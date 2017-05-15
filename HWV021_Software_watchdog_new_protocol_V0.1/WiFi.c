/****************************************************************************
 *   $Id:: WiFi.c  2015-06-10                    $
 *   Project: MagneticDetector
 *
 *   Description:
 *     This file contains WiFi code which include WiFi 
 *     initializationand related APIs for WiFi access.
****************************************************************************/
#include "LPC11xx.h"
#include "math.h"
#include "uart.h"
#include "WiFi.h"
#include "timer32.h"
#include "timer16.h"
#include "gpio.h"

uint8_t  WiFi_TxBuf[WIFI_TX_BUFSIZE],WiFi_MAC[12],WiFi_IP[4];

extern uint32_t UARTCount;
extern uint8_t  UARTBuffer[UART_BUFSIZE];

/*****************************************************************************
** Function name:		SetWiFi_AT
**
** Descriptions:		configure WiFi module to AT command mode
**
** parameters:			 None
** Returned value:	 status, 0--successed, -1--error
** 
*****************************************************************************/
int SetWiFi_COMM(uint8_t ATorSocket)
{
  int tmp=-1,i=0,j=0,k=0;
	//Enter the AT command mode
	//for (int i=0;i<3;i++)   //In C language, the variant can not be defined in "for" loop which can be done in C++
	
if(ATorSocket==AT)																																					//Set WiFi module into AT command mode
{
		UARTCount=0;
		WiFi_TxBuf[0]='+'; 
		for (i=0;i<3;i++) { UARTSend(WiFi_TxBuf,1);}																						//UARTSend(uint8_t *pBuffer, uint32_t Length)  //Transmit "+++"

			delay16Ms(0,1000);																																		//Delay 1000ms
			if(UARTCount>0  && UARTBuffer[0]=='a')   																							//check if 'a' is received 
			{
				WiFi_TxBuf[0]='a';
				UARTCount=0;
				UARTSend(WiFi_TxBuf,1);																															//Send 'a' to WiFi module

				delay16Ms(0,1000);																																	//Delay 1s			
				if(UARTCount>0 && UARTBuffer[0]=='+' && UARTBuffer[1]=='O' && (UARTBuffer[2]=='K')) //check if '+OK' is received 
				{
					tmp=0;
				}
		
			}
}
else if (ATorSocket==SOCKET)	//Set WiFi module into Socket mode
{
		UARTCount=0;					
		WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='E';WiFi_TxBuf[4]='N';WiFi_TxBuf[5]='T';WiFi_TxBuf[6]='M';WiFi_TxBuf[7]='\r';
		UARTSend(WiFi_TxBuf,8);	

		delay16Ms(0,1000);			//Delay 1000ms
		if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K')  {tmp=0;}
}
  return tmp;
}

/*****************************************************************************
** Function name:		SetCMDEcho
**
** Descriptions:		configure WiFi module to AP mode
**
** parameters:			 ON/OFF
** Returned value:	 status, 0--successed, -1--error
** 
*****************************************************************************/
int SetCMDEcho(uint8_t on_off)
{
  int tmp=-1,i=0;	
	if(on_off==ON)
		{
			UARTCount=0;					
			WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='E';WiFi_TxBuf[4]='=';WiFi_TxBuf[5]='o';WiFi_TxBuf[6]='n';WiFi_TxBuf[7]='\r';
			UARTSend(WiFi_TxBuf,8);
		}
		else if(on_off==OFF)
		{
			UARTCount=0;					
			WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='E';WiFi_TxBuf[4]='=';WiFi_TxBuf[5]='o';WiFi_TxBuf[6]='f';WiFi_TxBuf[7]='f';WiFi_TxBuf[8]='\r';
			UARTSend(WiFi_TxBuf,9);
		}
		
	for (i=0;i<3;i++)
		{
			delay16Ms(0,300);			//Delay 300ms
			if(UARTCount>6)
			{
				if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K')  {tmp=0;}
				break;
			}
		}
return tmp;
}

/*****************************************************************************
** Function name:		SetWiFiMode
**
** Descriptions:		configure WiFi module to AP mode
**
** parameters:			 WiFi_Mode:one of AP or STA
** Returned value:	 status, 0--successed, -1--error
** 
*****************************************************************************/
int SetWiFiMode(uint8_t WiFi_Mode)
{
  int tmp=-1,i=0;
	
	if(WiFi_Mode==AP)
	{
		UARTCount=0;					
		WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='W';WiFi_TxBuf[4]='M';WiFi_TxBuf[5]='O';WiFi_TxBuf[6]='D';WiFi_TxBuf[7]='E';WiFi_TxBuf[8]='=';WiFi_TxBuf[9]='A';WiFi_TxBuf[10]='P';WiFi_TxBuf[11]='\r';
		UARTSend(WiFi_TxBuf,12);	
	}
	else if(WiFi_Mode==STA)
	{
		UARTCount=0;					
		WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='W';WiFi_TxBuf[4]='M';WiFi_TxBuf[5]='O';WiFi_TxBuf[6]='D';WiFi_TxBuf[7]='E';WiFi_TxBuf[8]='=';WiFi_TxBuf[9]='S';WiFi_TxBuf[10]='T';WiFi_TxBuf[11]='A';WiFi_TxBuf[12]='\r';
		UARTSend(WiFi_TxBuf,13);	
	}

for (i=0;i<3;i++)
	{
		delay16Ms(0,300);			//Delay 300ms
		if(UARTCount>6)
		{
			if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K')  {tmp=0;}
			break;
		}
	}
return tmp;
}
	/*****************************************************************************
** Function name:		WiFiReset
**
** Descriptions:		Reset WiFi module through AT command
**
** parameters:			 No
** Returned value:	 status, 0--successed, -1--error
** 
*****************************************************************************/
int WiFiReset_SW(void)
{
  int tmp=-1,i=0;
	
	UARTCount=0;					
	WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='Z';WiFi_TxBuf[4]='\r';
	UARTSend(WiFi_TxBuf,5);	

for (i=0;i<3;i++)
	{
		delay16Ms(0,300);			//Delay 300ms
		if(UARTCount>6)
		{
			if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K')  {tmp=0;}
			break;		
		}

	}
	return tmp;
}

/*****************************************************************************
** Function name:		WiFiReset
**
** Descriptions:		Reset WiFi module through AT command
**
** parameters:			 No
** Returned value:	 status, 0--successed, -1--error
** 
*****************************************************************************/
void WiFiReset_HW(void)
{
	GPIOSetValue(PORT0,3,0);								//Reset WiFi module
	delay16Ms(0,1000);			//Delay 1000ms
	GPIOSetValue(PORT0,3,1);								//Release WiFi module from reset
//	delay16Ms(0,10000);			//Delay 10s
}

/*****************************************************************************
** Function name:		WiFiWModeQuery
**
** Descriptions:		Check the WiFi module is in AP mode or STA mode
**
** parameters:			 No
** Returned value:	 status, 0--successed, -1--error
** 
*****************************************************************************/
int WiFiWModeQuery(void)
{
  int tmp=-1,i=0;
	
	UARTCount=0;					
	WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='W';WiFi_TxBuf[4]='M';WiFi_TxBuf[5]='O';WiFi_TxBuf[6]='D';WiFi_TxBuf[7]='E';WiFi_TxBuf[8]='\r';
	UARTSend(WiFi_TxBuf,9);	

for (i=0;i<3;i++)
	{
		delay16Ms(0,300);			//Delay 300ms
		if(UARTCount>6)
		{
			if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K' && UARTBuffer[5]=='=' && UARTBuffer[6]=='A' && UARTBuffer[7]=='P' && UARTBuffer[8]=='\r')  {tmp=0;}	//AP mode
			else
				if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K' && UARTBuffer[5]=='=' && UARTBuffer[6]=='S' && UARTBuffer[7]=='T' && UARTBuffer[8]=='A' && UARTBuffer[9]=='\r')  {tmp=1;}	//STA mode
			break;		
		}

	}
	return tmp;
}

/*****************************************************************************
** Function name:		WiFiMACQuery
**
** Descriptions:		Query MAC address
**
** parameters:			 No
** Returned value:	 status, 0--successed, -1--error
** 
*****************************************************************************/
int WiFiMACQuery(void)
{
  int tmp=-1,i=0,k=0;
	UARTCount=0;					
	WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='M';WiFi_TxBuf[4]='A';WiFi_TxBuf[5]='C';WiFi_TxBuf[6]='\r';
	UARTSend(WiFi_TxBuf,7);	

	for (i=0;i<3;i++)		//maximum wait time is 3s for response
	{
		delay16Ms(0,300);			//Delay 1000ms
		if(UARTCount>6)
		{
			if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K')  
			{
				tmp=0;
				for(k=0;k<12;k++) {WiFi_MAC[k]=UARTBuffer[k+6];}		//MAC address, asic code
				break;	
			}						
		}
	}
	return tmp;
}

/*****************************************************************************
** Function name:		WiFiIPQuery
**
** Descriptions:		Query IP address
**
** parameters:			 No
** Returned value:	 status, 0--successed, -1--error
** 
*****************************************************************************/
int WiFiIPQuery(void)
{
  int tmp=-1;
	UARTCount=0;					
	WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='W';WiFi_TxBuf[4]='A';WiFi_TxBuf[5]='N';WiFi_TxBuf[6]='N';WiFi_TxBuf[7]='\r';
	UARTSend(WiFi_TxBuf,8);	
	
		delay16Ms(0,600);			//Delay 600ms
		if(UARTCount>6)
		{
			if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K')  
			{
				tmp=0;
				IP_BCD2BIN(UARTBuffer,WiFi_IP);
			}						
		}
	return tmp;
}

/*****************************************************************************
** Function name:		WiFi_LinkQuery(void)
**
** Descriptions:		Query if WiFi is in STA status and if it's linked to network
**
** parameters:			 No
** Returned value:	 status, 1--successed, 0--not linked
** 
*****************************************************************************/
int WiFi_LinkQuery(void)
{
  if (GPIOGetValue( PORT0,8 )==0) return 1;
	else return 0;
}

/*****************************************************************************
** Function name:		int IP_BCD2BIN(uint8_t *pBuffer_s,uint8_t *pBuffer_d)
**
** Descriptions:		Transfer the IP code to binary from BCD code
**
** parameters:			 pBuffer_s--IP address with BCD code; pBuffer_d----binary IP address
** Returned value:	 status, 0--successed, -1--Failure
** 
*****************************************************************************/
int IP_BCD2BIN(uint8_t *pBuffer_s,uint8_t *pBuffer_d)
{
	int tmp=-1,i=0,j=0,k=0,m=0;
	uint8_t	tmp_IP=0;

	/*Find the 1st digit*/
	for (j=0;j<30;j++)
	{
		if(pBuffer_s[j]>='0' && pBuffer_s[j]<='9') {k=j;break;}
	}
	
	/*BCD2BIN*/	
if(k>0&&k<30)
	{
	i=0;
	for (j=k;j<30;j++)					//find the next dot.	
		{			
			if(i<3)									//IP bytes
			{
				if(pBuffer_s[j]=='.')
					{
						tmp_IP=0;
						for(m=0;m<j-k;m++) {tmp_IP=tmp_IP+(pBuffer_s[j-m-1]-0x30)*pow(10,m);}						
						pBuffer_d[i]=tmp_IP;
						i++;k=j+1;									
					}	
			}
			else				
				if(pBuffer_s[j]<'0' || pBuffer_s[j]>'9')
					{
						tmp_IP=0;
						for(m=0;m<j-k;m++) {tmp_IP=tmp_IP+(pBuffer_s[j-m-1]-0x30)*pow(10,m);}
						pBuffer_d[i]=tmp_IP;	
						tmp=0;
						break;
					}							
		}
	}		
return tmp;		
}

/*****************************************************************************
** Function name:		void ByteBin2ASIC(uint8_t *pBuffer_s,uint8_t *pBuffer_d,uint8_t B_CNT)
**
** Descriptions:		Transfer byte-based binarycode to ASIC
**
** parameters:			 pBuffer_s--source binary code;  pBuffer_d--destimation for ASIC code, B_CNT--Bytes need to be transferred
** Returned value:	 status, 0--successed, -1--Failure
** 
*****************************************************************************/
void ByteBin2ASIC(uint8_t *pBuffer_s,uint8_t *pBuffer_d,uint8_t B_CNT)
{
	int i=0,j=0;
	uint8_t	tmp_asic[32];				//Temporary to store the asic code
	j=0;
	for (i=0;i<B_CNT;i++)
		{
			tmp_asic[j]=(pBuffer_s[i]>>4)&0xf;				//High 4 bits
			if(tmp_asic[j]<0xa) {tmp_asic[j]=tmp_asic[j]+0x30;}		//0~9
				else {tmp_asic[j]=tmp_asic[j]+0x37;}								//a~f
			
			j=j+1;
			tmp_asic[j]=pBuffer_s[i]&0xf;				//Low 4 bits
			if(tmp_asic[j]<0xa) {tmp_asic[j]=tmp_asic[j]+0x30;}		//0~9
				else {tmp_asic[j]=tmp_asic[j]+0x37;}								//a~f	
			j=j+1;
		}
	
	for(i=0;i<B_CNT*2;i++)
		{
			pBuffer_d[i]=tmp_asic[i];
		}			
return;		
}

/*****************************************************************************
** Function name:		void ByteASIC2Bin(uint8_t *pBuffer_s,uint8_t *pBuffer_d,uint8_t B_CNT)
**
** Descriptions:		Transfer ASIC code to HEX
**
** parameters:			 pBuffer_s--source binary code;  pBuffer_d--destimation for ASIC code, B_CNT--Bytes need to be transferred
** Returned value:	 status, 0--successed, -1--Failure
** 
*****************************************************************************/
void ByteASIC2Bin(uint8_t *pBuffer_s,uint8_t *pBuffer_d,uint8_t B_CNT)
{
	int i=0,j=0;
	uint8_t	tmp_asic[32];				//Temporary to store the asic code
	j=0;
	for (i=0;i<B_CNT>>1;i++)			//i--HEX, j--ASIC
		{
			tmp_asic[i]=pBuffer_s[j];
			if(tmp_asic[i]>0x2f && tmp_asic[i]<0x3a) {tmp_asic[i]=(tmp_asic[i]-0x30)*16;}		//0~9
				else {tmp_asic[i]=(tmp_asic[i]-0x37)*16;}								//a~f
			
			j=j+1;
				
			if(pBuffer_s[j]>0x30 && pBuffer_s[j]<0x3a) {tmp_asic[i]=tmp_asic[i]+pBuffer_s[j]-0x30;}		//0~9
			else {tmp_asic[i]=tmp_asic[i]+pBuffer_s[j]-0x37;}		
			j=j+1;
		}
	
	for(i=0;i<B_CNT>>1;i++)
		{
			pBuffer_d[i]=tmp_asic[i];
		}			
return;		
}

/*****************************************************************************
** Function name:		int REGENA(void)
**
** Descriptions:		Enable or disable register package head
**
** parameters:			no
** Returned value:	status, 0--successed, -1--Failure
** 
*****************************************************************************/
int REGENA_OFF(void)
{
	int tmp=-1;
	UARTCount=0;					
	WiFi_TxBuf[0]='A';WiFi_TxBuf[1]='T';WiFi_TxBuf[2]='+';WiFi_TxBuf[3]='R';WiFi_TxBuf[4]='E';WiFi_TxBuf[5]='G';WiFi_TxBuf[6]='E';WiFi_TxBuf[7]='N';WiFi_TxBuf[8]='A';WiFi_TxBuf[9]='=';WiFi_TxBuf[10]='O';WiFi_TxBuf[11]='F';WiFi_TxBuf[12]='F';WiFi_TxBuf[13]='\r';WiFi_TxBuf[14]='\n';
	UARTSend(WiFi_TxBuf,15);	
	
	delay16Ms(0,600);			//Delay 600ms
	if(UARTCount>6)
	{
		if(UARTBuffer[2]=='+' && UARTBuffer[3]=='O' && UARTBuffer[4]=='K')  
		{
			tmp=0;
		}						
	}
return tmp;		
}

/******************************************************************************
**                            End Of File
******************************************************************************/
