/****************************************************************************
 *   $Id:: uart.c 3648 2010-06-02 21:41:06Z usb00423                        $
 *   Project: NXP LPC11xx UART example
 *
 *   Description:
 *     This file contains UART code example which include UART 
 *     initialization, UART interrupt handler, and related APIs for 
 *     UART access.
 *
 ****************************************************************************
 * Software that is described herein is for illustrative purposes only
 * which provides customers with programming information regarding the
 * products. This software is supplied "AS IS" without any warranties.
 * NXP Semiconductors assumes no responsibility or liability for the
 * use of the software, conveys no license or title under any patent,
 * copyright, or mask work right to the product. NXP Semiconductors
 * reserves the right to make changes in the software without
 * notification. NXP Semiconductors also make no representation or
 * warranty that such application will be suitable for the specified
 * use without further testing or modification.
****************************************************************************/
#include "LPC11xx.h"
#include "math.h"
#include "uart.h"

volatile uint32_t UARTStatus;
volatile uint8_t  UARTTxEmpty = 1;
volatile uint8_t  UARTBuffer[UART_BUFSIZE];
volatile uint32_t UARTCount = 0;

void BaudRateCalculator(uint32_t uartclk, uint32_t baudrate,uint32_t* dl,uint32_t* dval,uint32_t* mval)
{
	int temp,td;
	int rate16 = 16 * baudrate;    
	int deltaMin,d,delta,i,j;
	int rem = (int)(uartclk % rate16);
	int div = (int)(uartclk / rate16);
	*dval = 0;
	*mval = 1;
	*dl = div;

	if (rem > 0)
	{
		deltaMin = -1;
		for (i = div/2 + 1; i <= div; i++)
		{
				temp = rate16*i;
				td = uartclk - temp;
				for (j = 1; j < 16; j++)
				{
						d = (int)rint(td * (float)j / temp);
						delta = (int)fabs(d*temp - td*j);
						if (deltaMin == -1 || delta < deltaMin)
						{
								deltaMin = delta;
								*dval = d;
								*mval = j;
								*dl = i;
						}
				}
		}
	}
}

/*****************************************************************************
** Function name:		UART_IRQHandler
**
** Descriptions:		UART interrupt handler
**
** parameters:			None
** Returned value:		None
** 
*****************************************************************************/
void UART_IRQHandler(void)
{
  uint8_t IIRValue, LSRValue;
  uint8_t Dummy = Dummy;

  IIRValue = LPC_UART->IIR;
    
  IIRValue >>= 1;			/* skip pending bit in IIR */
  IIRValue &= 0x07;			/* check bit 1~3, interrupt identification */
  if (IIRValue == IIR_RLS)		/* Receive Line Status */
  {
    LSRValue = LPC_UART->LSR;
    /* Receive Line Status */
    if (LSRValue & (LSR_OE | LSR_PE | LSR_FE | LSR_RXFE | LSR_BI))
    {
      /* There are errors or break interrupt */
      /* Read LSR will clear the interrupt */
      UARTStatus = LSRValue;
      Dummy = LPC_UART->RBR;	/* Dummy read on RX to clear 
								interrupt, then bail out */
      return;
    }
    if (LSRValue & LSR_RDR)	/* Receive Data Ready */			
    {
      /* If no error on RLS, normal ready, save into the data buffer. */
      /* Note: read RBR will clear the interrupt */
      UARTBuffer[UARTCount++] = LPC_UART->RBR;
      if (UARTCount == UART_BUFSIZE)
      {
        UARTCount = 0;		/* buffer overflow */
      }	
    }
  }
  else if (IIRValue == IIR_RDA)	/* Receive Data Available */
  {
    /* Receive Data Available */
    UARTBuffer[UARTCount++] = LPC_UART->RBR;
    if (UARTCount == UART_BUFSIZE)
    {
      UARTCount = 0;		/* buffer overflow */
    }
  }
  else if (IIRValue == IIR_CTI)	/* Character timeout indicator */
  {
    /* Character Time-out indicator */
    UARTStatus |= 0x100;		/* Bit 9 as the CTI error */
  }
  else if (IIRValue == IIR_THRE)	/* THRE, transmit holding register empty */
  {
    /* THRE interrupt */
    LSRValue = LPC_UART->LSR;		/* Check status in the LSR to see if
								valid data in U0THR or not */
    if (LSRValue & LSR_THRE)
    {
      UARTTxEmpty = 1;
    }
    else
    {
      UARTTxEmpty = 0;
    }
  }
  return;
}

/*****************************************************************************
** Function name:		UARTInit
**
** Descriptions:		Initialize UART0 port, setup pin select,
**				clock, parity, stop bits, FIFO, etc.
**
** parameters:			UART baudrate
** Returned value:		None
** 
*****************************************************************************/
void UARTInit(uint32_t baudrate)
{
  uint32_t Fdiv,Dval,Mval;
  uint32_t regVal;

  //UARTTxEmpty = 1;
  UARTCount = 0;
  
  NVIC_DisableIRQ(UART_IRQn);

  LPC_IOCON->PIO1_6 &= ~0x07;    /*  UART I/O config */
  LPC_IOCON->PIO1_6 |= 0x01;     /* UART RXD */
  LPC_IOCON->PIO1_7 &= ~0x07;	
  LPC_IOCON->PIO1_7 |= 0x01;     /* UART TXD */

  /* Enable UART clock */
  LPC_SYSCON->SYSAHBCLKCTRL |= (1<<12);
  LPC_SYSCON->UARTCLKDIV = 0x1;     /* divided by 1 */

  LPC_UART->LCR = 0x83;             /* 8 bits, no Parity, 1 Stop bit */
	
	regVal = LPC_SYSCON->UARTCLKDIV;
  //Fdiv = ((SystemAHBFrequency/regVal)/16)/baudrate ;	/*baud rate */
	
	BaudRateCalculator(SystemCoreClock/regVal,baudrate,&Fdiv,&Dval,&Mval);

  LPC_UART->DLM = Fdiv >> 8;							
  LPC_UART->DLL = Fdiv & 0xff;
	LPC_UART->FDR = Mval << 4 | Dval;
  LPC_UART->LCR = 0x03;		/* DLAB = 0 */
  LPC_UART->FCR = 0x07;		/* Enable and reset TX and RX FIFO. */

  /* Read to clear the line status. */
  regVal = LPC_UART->LSR;

  /* Ensure a clean start, no data in either TX or RX FIFO. */
  while (( LPC_UART->LSR & (LSR_THRE|LSR_TEMT)) != (LSR_THRE|LSR_TEMT) );
  while ( LPC_UART->LSR & LSR_RDR )
  {
	regVal = LPC_UART->RBR;	/* Dump data from RX FIFO */
  }
 
  /* Enable the UART Interrupt */
  NVIC_EnableIRQ(UART_IRQn);

#if TX_INTERRUPT
  LPC_UART->IER = IER_RBR | IER_THRE | IER_RLS;	/* Enable UART interrupt */
#else
  LPC_UART->IER = IER_RBR | IER_RLS;	/* Enable UART interrupt */
#endif
  return;
}

/*****************************************************************************
** Function name:		UARTSend
**
** Descriptions:		Send a block of data to the UART 0 port based
**				on the data length
**
** parameters:		buffer pointer, and data length
** Returned value:	None
** 
*****************************************************************************/
void UARTSend(uint8_t *pBuffer, uint32_t Length)
{
  int i=0;
  while (i<Length)
  {
	  /* THRE status, contain valid data */
#if !TX_INTERRUPT
	  while ( !(LPC_UART->LSR & LSR_THRE) );
	  LPC_UART->THR = pBuffer[i++];
#else
	  /* Below flag is set inside the interrupt handler when THRE occurs. */
    while ( !(UARTTxEmpty & 0x01) );
	  LPC_UART->THR = pBuffer[i++];
    UARTTxEmpty = 0;	/* not empty in the THR until it shifts out */
#endif
  }
  return;
}

void UARTSendSpecial(uint8_t *BufferPtr, uint16_t Length)
{
  uint32_t i;
	uint8_t header[4];
#if !TX_INTERRUPT
	  while ( !(LPC_UART->LSR & LSR_THRE) );
#else
	  /* Below flag is set inside the interrupt handler when THRE occurs. */
    while ( !(UARTTxEmpty & 0x01) );
#endif
    /* Receive Line Status */
	//while ((LPC_UART->LSR & LSR_RDR) == LSR_RDR);
	
	header[0]=MSG_HEADER;
	header[1]=Length & 0xff;
	header[2]=Length >> 8;
	header[3]=header[0]+header[1]+header[2];
	
	for ( i = 0; i < 3; i++ )
  {
#if !TX_INTERRUPT
	  while ( !(LPC_UART->LSR & LSR_THRE) );
#else
	  /* Below flag is set inside the interrupt handler when THRE occurs. */
    while ( !(UARTTxEmpty & 0x01) );
#endif
		LPC_UART->THR = header[i];
    UARTTxEmpty = 0;
	}
	
  for ( i = 0; i < Length; i++ )
  {
	/* THRE status, contain valid data */
		header[3]+=BufferPtr[i];
#if !TX_INTERRUPT
	  while ( !(LPC_UART->LSR & LSR_THRE) );
#else
	  /* Below flag is set inside the interrupt handler when THRE occurs. */
    while ( !(UARTTxEmpty & 0x01) );
#endif
		LPC_UART->THR = BufferPtr[i];
    UARTTxEmpty = 0;	/* not empty in the THR until it shifts out */
  }
	
#if !TX_INTERRUPT
	while ( !(LPC_UART->LSR & LSR_THRE) );
#else
	  /* Below flag is set inside the interrupt handler when THRE occurs. */
  while ( !(UARTTxEmpty & 0x01) );
#endif
	LPC_UART->THR = header[3];
  UARTTxEmpty = 0;
	
	#if !TX_INTERRUPT
	while ( !(LPC_UART->LSR & LSR_THRE) ) {}
#else
	  /* Below flag is set inside the interrupt handler when THRE occurs. */
  while ( !(UARTTxEmpty & 0x01) ) {}
#endif
  return;
}

/******************************************************************************
**                            End Of File
******************************************************************************/
