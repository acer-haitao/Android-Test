#include <LPC11xx.h>
#include <cstdint>
#include <cstring>
#include <cmath>
#include "gpio.h"
#include "uart.h"
#include "timer32.h"
#include "timer16.h"
#include "WiFi.h"
#include <string>

using namespace std;

#define TIMEOUT 			100
#define MAG_INTERVAL_InMS				0.625													//Interval to between two samples (32 points per period with respect to 50Hz. Out of the 32 samples, 8 samples will be used to caculate the magnetic strength)
#define UDP_INTERVAL_InMS					5000												//The time interval transferring data to server						
#define MagAMPBuffSize int(20/MAG_INTERVAL_InMS+0.5)*2				//MagAMPBuff matrix size of float
#define pi												3.1415926			
#define LED_ON										1			
#define LED_OFF										0
#define FUNCTION_UART																		//UART as the default port to transfer data 
//#define DEBUG																						//If defined, the debug function is ON

extern uint32_t UARTStatus;
extern uint8_t 	UARTBuffer[UART_BUFSIZE];
extern uint32_t UARTCount;
extern uint32_t timer32_1_counter;
extern uint32_t timer32_0_counter;
extern uint8_t  WiFi_TxBuf[WIFI_TX_BUFSIZE];
extern uint8_t 	WiFi_MAC[12];

const float sin50t_32[32]={0,0.195090,0.382683,0.555570,0.707107,0.831470,0.923880,0.980785,1.0,0.980785,0.923880,0.831470,0.707107,0.555570,0.382683,0.195090,0.0,-0.195090,-0.382683,-0.555570,-0.707107,-0.831470,-0.923880,-0.980785,-1.0,-0.980785,-0.923880,-0.831470,-0.707107,-0.555570,-0.382683,-0.195090};
const uint16_t crc_table[16] ={0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7, 0x8108, 0x9129, 0xa14a, 0xb16b,	0xc18c, 0xd1ad, 0xe1ce, 0xf1ef};

void GreenLED_On(void)
{
	GPIOSetValue(PORT0,2,0);	  //Green LED 
	return;
}

void GreenLED_Off(void)
{
	GPIOSetValue(PORT0,2,1);	  //Green LED 
	return;
}

uint8_t GetGreenLedStatus(void)
{
	uint32_t tmp=0;
	tmp=GPIOGetValue( 0, 2);				//uint32_t GPIOGetValue( uint32_t portNum, uint32_t bitPosi )
	if(tmp&0x02)
	{
		tmp=0;
	}
	else		
	{
		tmp=1;
	}
	return	tmp;
}

void RedLED_On(void)
{
	GPIOSetValue(PORT0,3,0);	  //Red LED 
	return;
}

void RedLED_Off(void)
{
	GPIOSetValue(PORT0,3,1);	  //Red LED 
	return;
}

void BlueLED_On(void)
{
	GPIOSetValue(PORT0,8,0);	  //Blue LED 
	return;
}

void BlueLED_Off(void)
{
	GPIOSetValue(PORT0,8,1);	  //Blue LED 
	return;
}

void enable_watchdog(uint16_t TimeInSecond)			//should less than 350s in 48MHz system clock
{
//Power: In the SYSAHBCLKCTRL register, set bit 15.
	LPC_SYSCON->SYSAHBCLKCTRL |= (1<<15);		//Enables clock for watchdog
	
//Peripheral clock: Select the WDT clock source (Table 25)
	//defalt, IRC clock
	
//enable the WDT peripheral clock by writing to the WDTCLKDIV register (Table 27).
	LPC_SYSCON ->CLKOUTDIV=0xff;								//watchdog clock=IRC clock/255
	
//Set the Watchdog timer constant reload value in WDTC register.
	LPC_WDT ->TC=TimeInSecond*SystemCoreClock/((LPC_SYSCON ->CLKOUTDIV)*4);   //
	
//Setup the Watchdog timer operating mode in WDMOD register.
	LPC_WDT ->MOD	&=~0x3;
	LPC_WDT ->MOD	|=~0x3;								//Reset the chip when watchdog time out. 
	
//Set a value for the watchdog window time in WDWINDOW register if windowed operation is required.
	
//Set a value for the watchdog warning interrupt in the WDWARNINT register if a warning interrupt is required.
	
//Enable the Watchdog by writing 0xAA followed by 0x55 to the WDFEED register.
LPC_WDT ->FEED=0xAA; LPC_WDT ->FEED=0x55;

//The Watchdog must be fed again before the Watchdog counter reaches zero in order
//to prevent a watchdog event. If a window value is programmed, the feed must also
//occur after the watchdog counter passes that value.	
}

uint16_t TransmitCrc16(uint8_t* ptr , int len)
{
	uint16_t crc = 0;
	uint8_t crc_H4;
	while( len-- )
	{
		crc_H4 = (uint8_t)(crc >> 12);
		crc = crc << 4;
		crc = crc ^ crc_table[ crc_H4 ] ^ (*ptr >> 4);
		crc_H4 = (uint8_t)(crc >> 12);
		crc = crc << 4;
		crc = crc ^ crc_table[ crc_H4 ] ^ (*ptr & 0x0f);
		ptr++;
	}
	return crc;
}

void TransmitSend(uint8_t* data,int len)
{
	uint16_t crc = TransmitCrc16(data+6,len-8);
	*(data + len - 2) = crc;
	*(data + len - 1)  = crc>>8;
#if defined FUNCTION_CAN
		CANSend(data,len);
#elif defined FUNCTION_UART
		UARTSend(data ,len);	
#endif
}


void feedwdt(void)
{
	LPC_WDT ->FEED=0xAA; 
	LPC_WDT ->FEED=0x55;
}

void OnError(int16_t ErrorID)
{
	while (1);				
}

uint16_t 	SizeofMagAMPBuff=MagAMPBuffSize;													//Sample matrix size of float (32bit)
uint16_t 	SizeofCapTCBuff=SizeofMagAMPBuff+1;												//TC value bufffer
uint16_t 	SamplesPerPeriod=int(20/MAG_INTERVAL_InMS+0.5);						//The sample counts per 20ms
float 		Indicator_X1=0.0;																					//The magnetic strength indicator in one cycle
uint32_t 	Cap_TC_Buff[MagAMPBuffSize+1];													//TC- value for the sampes
//uint32_t 	Cap_TC_Buff1[MagAMPBuffSize];														//TC+ value for the sampes
uint32_t 	Cap_CNT_Buff[MagAMPBuffSize+1];														//Delata Capture counts for the sampes

union
{
float			MagAMP_float[MagAMPBuffSize];																
uint8_t		MagAMP_UART[MagAMPBuffSize*4];
}MagAMPBuff;																												//Store the measured magnetic strength in system frequency counts						
	
#ifdef DEBUG
union
{
uint32_t 	MagAMP_uint32[MagAMPBuffSize];
uint8_t		MagAMP_UART[MagAMPBuffSize*4];	
}
MagAMPCopy;							//When debugging, is is used to transfer MagAMPBuff to Integger with scale of 1000000
#endif

union
{
	float 		MagAMP_Float;
	uint8_t		MagAMP_UART[4];
} MagAMP;																										//Average Magnatic Strength sending to server
uint16_t	MagAMPBuff_Index;																					//The index for MagAMPBuff
uint16_t	IndicatorBufIndex;																				//The index for average Indicator_X1

uint32_t 	timer32_1_cap_cnt=0;							//Current count of magnetic clock
uint8_t		Timer32_1_cap_cnt_tc_en=0;		//when 1, enable the snapshot of capture counter- and tc-

int main(void)																											//cdh, the main entrance for the user code
{		
	SystemCoreClockUpdate();
	GPIOInit();																												//[cdh] Initiate the GPIO
	UARTInit(115200);																									//initiate UART with baudrate of 115200

	init_timer32(1, 1000);
	init_timer32(0, MAG_INTERVAL_InMS*(SystemCoreClock / 1000));  		//MAG_INTERVAL_InMS ms intervals
	init_timer16(0, 300);  																						//300ms for delay

//	enable_timer32(0);
//	enable_timer32(1);
	enable_timer16(0);
//	enable_watchdog(300);							//set watchdog duration with 300s
		
	int tmp_f=0,AT_Indicator=0;	
	uint8_t TOGGLE_AP2SOCKET=1;																					//when 1, change WiFi mode to STA
	
	float MagAMP_Tmp=0.0;																								//Temperarily store the Magnetic strength
	uint16_t Tx_Interval_InCycles=UDP_INTERVAL_InMS/MAG_INTERVAL_InMS;	//Data transfer to server interval with cycles

	IndicatorBufIndex=0;
	timer32_0_counter=0;
	
	
//Wait WiFi module ready
	WiFiReset_HW();	
	while (GPIOGetValue(PORT0,9)!=0);

//Set WiFi UART0 in AT command mode
	tmp_f=SetWiFi_COMM(AT);		//Set UART communication in AT command mode
	if (tmp_f!=0)
	{
		tmp_f=SetWiFi_COMM(AT);		//Retry again if failure
	}
	if (tmp_f==0)
	{
		SetCMDEcho(OFF);
	}
	else
	{
		OnError(tmp_f);
	}

//Set WiFi module to AP mode if WiFi_Config is low
	if (GPIOGetValue(PORT1,2)==0) 																	//If WiFi module ready and WiFi Configuration pin is LOW, set WiFi mode to AP mode
		{		
//			disable_timer32(0);
//			disable_timer32(1);				//Disable timer320/1 to keep the UART command from disterbing
			
			
			if(WiFiWModeQuery()!=AP)				//WiFi module is not in AP mode
			{
				SetWiFiMode(AP);
				//WiFiReset_HW();	
			}
			
			while(1)
			{
			BlueLED_Off();
			delay16Ms(0,1000);					//Delay 1000ms
			BlueLED_On();
			delay16Ms(0,1000);					//Delay 1000ms
			}
		}	
		else
		{		
      SetWiFiMode(AP);                 //yb
			if(WiFiWModeQuery()!=STA)				//WiFi module is not in STA mode
			{
				SetWiFiMode(STA);
				SetWiFi_COMM(SOCKET);					//exit AT command mode and enter into socket mode
				WiFiReset_HW();	 
			}						
		}
						
		enable_timer32(0);
		enable_timer32(1);				//enable timer320/1
	


while(1)
	{		
	//	feedwdt();					//feed watchdog
//		while((GPIOGetValue(PORT0,1)==0)&(GPIOGetValue(PORT0,9)==0)) 																	/*If WiFi module ready and WiFi Configuration pin is LOW, set WiFi mode to AP mode*/
//		{		
//			disable_timer32(0);
//			disable_timer32(1);				//Disable timer320/1 to keep the UART command from disterbing
//			feedwdt();				//feed watchdog
//			delay16Ms(0,1000);																							//Delay 1000ms for avoiding jitter
//			if(GPIOGetValue(PORT1,2)==0)
//			{
//				TOGGLE_AP2SOCKET=1;
//				if (AT_Indicator==0) 
//				{				
//						tmp_f=SetWiFi_COMM(AT);
//						if (tmp_f==0) 
//							{
//								AT_Indicator=1;
//								SetCMDEcho(OFF);					
//							}	
//						else
//							{
//								BlueLED_Off();
//								RedLED_On();
//								delay16Ms(0,1000);					//Delay 1000ms
//								RedLED_Off();
//								delay16Ms(0,1000);					//Delay 1000ms
//							}						
//				}		//Set WiFi module in AT mode
//				
//				
//				if (AT_Indicator==1)
//				{
//					if(WiFiWModeQuery()==AP)				//WiFi module is in AP mode
//					{
//						BlueLED_On();
//						RedLED_Off();
//						delay16Ms(0,1000);					//Delay 1000ms
//						BlueLED_Off();
//						delay16Ms(0,1000);					//Delay 1000ms
//					}
//					else
//					{
//						SetWiFiMode(AP);
//						WiFiReset_HW();	
//						AT_Indicator=0;
//					}
//				}
//			}
//			enable_timer32(0);
//			enable_timer32(1);				//enable timer320/1
//		}
		

/****************************************************************************************************************/		
/*                                     Normal progress                                                         */
/**************************************************************************************************************/			
//		if((TOGGLE_AP2SOCKET==1)&(GPIOGetValue(PORT0,9)==0))			//when WiFI module is ready and  TOGGLE_AP2SOCKET is 1, need to change WiFi to socket mode at beginning of normal progress
//		{
//			/*Set WiFi module in AT mode*/			
//			if (AT_Indicator==0) 
//			{
//				tmp_f=SetWiFi_COMM(AT);
//				if (tmp_f==0) 
//					{
//						AT_Indicator=1;
//						SetCMDEcho(OFF);					
//					}
//			}		//Set WiFi module in AT mode
//			
//			if (AT_Indicator==1)
//			{
//				WiFiMACQuery();
//				REGENA_OFF();										//Witch off Register package head
//				if(WiFiWModeQuery()==AP) 
//				{
//					SetWiFiMode(STA); 
//					SetWiFi_COMM(SOCKET);					//exit AT command mode and enter into socket mode
//					WiFiReset_HW();
//				}
//				else
//				{
//					SetWiFi_COMM(SOCKET);					//exit AT command mode and enter into socket mode	
//				}		
//				AT_Indicator=0;
//				TOGGLE_AP2SOCKET=0;
//			}
//		}
					
		/*Caculate the magnetic strength*/
		if(MagAMPBuff_Index>=SizeofCapTCBuff)		//CapTCBuff is ready
		{
					
			for (int i=0;i<(SamplesPerPeriod<<1);i++)
			{
				if(Cap_TC_Buff[i]<Cap_TC_Buff[i+1])
				{
					Cap_TC_Buff[i]=Cap_TC_Buff[i+1]-Cap_TC_Buff[i];									//Magnetic strength with system frequency counts
				}
				else
				{
					Cap_TC_Buff[i]=0xFFFFFFFF-Cap_TC_Buff[i]+Cap_TC_Buff[i+1]+1;				//Magnetic strength with system frequency counts
				}			
				
				if(Cap_CNT_Buff[i]<Cap_CNT_Buff[i+1])
				{
					Cap_CNT_Buff[i]=Cap_CNT_Buff[i+1]-Cap_CNT_Buff[i];									//Magnetic strength with system frequency counts
				}
				else
				{
					Cap_CNT_Buff[i]=0xFFFFFFFF-Cap_CNT_Buff[i]+Cap_CNT_Buff[i+1]+1;				//Magnetic strength with system frequency counts
				}			
				
				MagAMPBuff.MagAMP_float[i]=(float)Cap_TC_Buff[i]/Cap_CNT_Buff[i];
				
			}
		///*Calculate the maximum correlation value
			float Max_Mag_Indicator = 0;  //Store the maximum sample data
			float	Tmp_Mag_Indicator = 0;	//Store the temperary sample data 
			for (int j=0;j<SamplesPerPeriod;j++)
			{
				Tmp_Mag_Indicator=0.0;
				for (int i=0;i<SamplesPerPeriod;i++)		//Calculate the correlation of 32 samples
				{	
					Tmp_Mag_Indicator=Tmp_Mag_Indicator+MagAMPBuff.MagAMP_float[j+i]*sin50t_32[i];
				}		
				if(Max_Mag_Indicator<Tmp_Mag_Indicator) {Max_Mag_Indicator=Tmp_Mag_Indicator;}
			}
		
				Indicator_X1=Max_Mag_Indicator; 		//The maximum magnetic strength indicator
////Sync the samples, correlation method		*/
			
		
//#ifdef DEBUG			//Debug only
//			//test
//			for(int k=0;k<SamplesPerPeriod*2;k++)//test
//			{
//			MagAMPCopy.MagAMP_uint32[k]=MagAMPBuff.MagAMP_float[k]*1000000;//test
//			}
//			UARTSend(MagAMPCopy.MagAMP_UART,SamplesPerPeriod*2*4);	//test
//			WiFi_TxBuf[0]=0x55;												//test
//			WiFi_TxBuf[1]=0xaa;												//test
//			UARTSend(WiFi_TxBuf, 2);									//test		
//			//test
//#endif
//			
//		//}
//#ifndef DEBUG 			
////////*Average   */			

		if(timer32_0_counter<Tx_Interval_InCycles)							
		{
			MagAMP_Tmp=MagAMP_Tmp+Indicator_X1;		//Store the sum of magnetic samples per cycle
			IndicatorBufIndex++;   
		}
		else
		{
			MagAMP.MagAMP_Float=MagAMP_Tmp/IndicatorBufIndex;   //Magnetic average strength
			
			int i=0;  
			WiFi_TxBuf[i++]=0x55;
			WiFi_TxBuf[i++]=0xaa;
			
			WiFi_TxBuf[i++]=0;								//Destination addr
			WiFi_TxBuf[i++]=0;								//Destination addr
			
			WiFi_TxBuf[i++]=18;								//Command length£¬ low byte
			WiFi_TxBuf[i++]=0;								//Command length£¬ high byte
			
			WiFi_TxBuf[i++]=0x04;							//Command, low byte
			WiFi_TxBuf[i++]=0x11;							//Command, high byte

			for (int j=0;j<12;j++)						//MAC address
			{
			WiFi_TxBuf[i++]=WiFi_MAC[j];							
			}
			WiFi_TxBuf[i++]=MagAMP.MagAMP_UART[0];
			WiFi_TxBuf[i++]=MagAMP.MagAMP_UART[1];
			WiFi_TxBuf[i++]=MagAMP.MagAMP_UART[2];
			WiFi_TxBuf[i++]=MagAMP.MagAMP_UART[3];							//little endian
			
			/*If WiFi module link is active*/
			if (GPIOGetValue( PORT1,1 )==0)
			{		
				TransmitSend(WiFi_TxBuf,i+2);										 //Send data
				BlueLED_On();
			}
			else
			{
				BlueLED_Off();												//Switch off the Blue LED
			}
			
			IndicatorBufIndex=0;
			MagAMP_Tmp=0;																					//Reset the MagAMP for new data
			timer32_0_counter=0;
		}
//#endif
		MagAMPBuff_Index=0;
		}	
	}
}

