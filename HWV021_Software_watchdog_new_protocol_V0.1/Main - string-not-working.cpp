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
#define MAG_COUNTER								100													//The number to counter the magnetic oscillating per sample
#define MAG_INTERVAL_InMS					0.625												//Interval to between two samples (32 points per period with respect to 50Hz. Out of the 32 samples, 8 samples will be used to caculate the magnetic strength)
#define UDP_INTERVAL_InMS					10000												//The time interval between UPD upload data							
#define MagAMPBuffSize int(20/MAG_INTERVAL_InMS+0.5)*2
#define MAG_STANDBY_MAX_CNT				20													//The counter of countinously detecting that the strength of measured magnetic is above the threthold
#define MAG_WORKING_MAX_CNT				20
#define MAG_OFF_MAX_CNT						20													//The counter of countinously detecting that the strength of measured magnetic is below the threthold
#define pi												3.1415926			
#define LED_ON										1			
#define LED_OFF										0
#define MAG_STANDBY_THRESHOLD			2														//Defined the AC power plugged in threthold of magnetic strength (OFF-->Stanby)
#define MAG_WORKING_THRESHOLD			1.5													//Defined the ON threthold of magnetic strength (standby-->working)

extern uint32_t UARTStatus;
extern uint8_t 	UARTBuffer[UART_BUFSIZE];
extern uint32_t UARTCount;
extern uint32_t timer32_1_counter;
extern uint32_t timer32_0_counter;
extern uint8_t  WiFi_TxBuf[WIFI_TX_BUFSIZE];
extern uint8_t 	WiFi_MAC[12];

const double sin50t_8[8]={sin(-pi/2),sin(-pi/4),0,sin(pi/4),1,sin(3*pi/4),0,sin(5*pi/4)};

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
	GPIOSetValue(PORT0,3,0);	  //Green LED 
	return;
}

void RedLED_Off(void)
{
	GPIOSetValue(PORT0,3,1);	  //Green LED 
	return;
}

void BlueLED_On(void)
{
	GPIOSetValue(PORT0,2,0);	  //Green LED 
	return;
}

void BlueLED_Off(void)
{
	GPIOSetValue(PORT0,2,1);	  //Green LED 
	return;
}

void JSON_UDP_Send(string *payload)			//str_tmp will hold the payload string
{

	/*Send MAC address*/
	//string str_tmp="{\"MACAddr\":\"\r";		//"MACAddr":"

}

uint32_t 	MagAMPBuff[MagAMPBuffSize];																//Store the measured magnetic timer number, 20--20ms, which is the cycle of 50Hz
uint32_t 	MagAMPCopy[MagAMPBuffSize];																//Store the data copied from MagAMPBuff used for magnetic calculation
uint16_t 	SizeofMagAMPBuff=MagAMPBuffSize;	
uint8_t		LED_On_Off=0;																							//Magnetic channel indicator
uint32_t 	TC_Tmp=0;																									//Record the TC temperarioy
uint8_t 	SamplesPerPeriod=int(20/MAG_INTERVAL_InMS+0.5);						//The sample numbers per 20ms
double 		Indicator_X1=0.0;																					//The magnetic strength indicator
uint16_t 	UARTCount_Tx=0;	
uint8_t		UDPCMD_Len=0;																							//UDP command length

int main(void)																											//cdh, the main entrance for the user code
{		
	SystemCoreClockUpdate();
	GPIOInit();																												//[cdh] Initiate the GPIO
	memset(MagAMPBuff,0,sizeof(MagAMPBuff));													//initiate the matrix	with all 0
	
	UARTInit(115200);																									//initiate UART with baudrate of 115200

	init_timer32(1, MAG_COUNTER);
	init_timer32(0, MAG_INTERVAL_InMS*(SystemCoreClock / 1000));  		//MAG_INTERVAL_InMS ms intervals
	init_timer16(0, 300);  																						//300ms for delay

	enable_timer32(0);
	enable_timer32(1);
	enable_timer16(0);
	
	uint8_t	MagMeasureStandbyCounter=0, MagMeasureStandbyOffCounter=0;	//The counter of continously measured On or Off of magnetic strength
	uint8_t	MagMeasureWorkingCounter=0;																	//The counter of continously measured working or Not working of magnetic strength
	
	int tmp_f=0,AT_Indicator=0;	
	int Timer0Cycle2UDP=UDP_INTERVAL_InMS/MAG_INTERVAL_InMS;						//Timer0 conters to upload data through UDP
	uint8_t TOGGLE_AP2SOCKET=1;																					//when 1, change WiFi mode to STA
	while(1)
	{		
		string itmp=POWEROFF;
		string *tmp=&itmp;
		JSON_UDP_Send(tmp);	
	}
}
