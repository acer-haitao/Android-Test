/****************************************************************************
 *   $Id:: timer32.c 3635 2010-06-02 00:31:46Z usb00423                     $
 *   Project: NXP LPC11xx 32-bit timer example
 *
 *   Description:
 *     This file contains 32-bit timer code example which include timer 
 *     initialization, timer interrupt handler, and related APIs for 
 *     timer setup.
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
#include "timer32.h"
#include "gpio.h"

volatile uint32_t timer32_0_counter = 0;
volatile uint32_t timer32_1_counter = 0;
volatile uint32_t timer32_0_capture = 0;
volatile uint32_t timer32_1_capture = 0;
volatile uint32_t timer32_0_period = 0;
volatile uint32_t timer32_1_period = 0;

extern uint8_t 	Channel_Index;
extern uint32_t	MagAMPBuff[];
extern uint32_t	TC_Tmp;
extern uint16_t SamplesPerPeriod;	//The sample numbers per
extern uint16_t SizeofMagAMPBuff;	
extern	uint16_t 	SizeofCapTCBuff;

extern	uint32_t 	timer32_1_cap_cnt;							//Current count of magnetic clock
//extern	uint32_t timer32_1_cap_tc;						//Current TC when rise edge of capture clock
//extern	uint32_t	timer32_1_cap_cnt0;					//cap_cnt-
//extern	uint32_t 	timer32_1_cap_tc0;					//cap_tc-

//extern 	uint32_t	timer32_1_cap_cnt0;					//cap_cnt-
//extern	uint32_t 	timer32_1_cap_tc0;					//cap_tc-
//extern	uint32_t	timer32_1_cap_cnt1;					//cap_cnt+
//extern	uint32_t 	timer32_1_cap_tc1;					//cap_tc+
extern	uint8_t		Timer32_1_cap_cnt_tc_en;		//when 1, enable the snapshot of capture counter and tc
//extern	uint8_t		Timer32_1_cap_cnt_tc1_en;		//when 1, enable the snapshot of capture counter+ and tc+
//extern	uint8_t		SampleDataReady;						//When 1, thw sample data is ready to be stored

extern uint32_t 	Cap_TC_Buff[];															//TC- value for the sampes
//extern uint32_t 	Cap_TC_Buff1[];															//TC+ value for the sampes
extern uint32_t 	Cap_CNT_Buff[];															//Delata Capture counts for the sampes
extern uint16_t		MagAMPBuff_Index;

/*****************************************************************************
** Function name:		delay32Ms
**
** Descriptions:		Start the timer delay in milo seconds
**				until elapsed
**
** parameters:		timer number, Delay value in milo second			 
** 						
** Returned value:	None
** 
*****************************************************************************/
void delay32Ms(uint8_t timer_num, uint32_t delayInMs)
{
  if (timer_num == 0)
  {
    /* setup timer #0 for delay */
    LPC_TMR32B0->TCR = 0x02;		/* reset timer */
    LPC_TMR32B0->PR  = 0x00;		/* set prescaler to zero */
    LPC_TMR32B0->MR0 = delayInMs * (SystemCoreClock / 1000);
    LPC_TMR32B0->IR  = 0xff;		/* reset all interrrupts */
    LPC_TMR32B0->MCR = 0x04;		/* stop timer on match */
    LPC_TMR32B0->TCR = 0x01;		/* start timer */
  
    /* wait until delay time has elapsed */
    while (LPC_TMR32B0->TCR & 0x01);
  }
  else if (timer_num == 1)
  {
    /* setup timer #1 for delay */
    LPC_TMR32B1->TCR = 0x02;		/* reset timer */
    LPC_TMR32B1->PR  = 0x00;		/* set prescaler to zero */
    LPC_TMR32B1->MR0 = delayInMs * (SystemCoreClock / 1000);
    LPC_TMR32B1->IR  = 0xff;		/* reset all interrrupts */
    LPC_TMR32B1->MCR = 0x04;		/* stop timer on match */
    LPC_TMR32B1->TCR = 0x01;		/* start timer */
  
    /* wait until delay time has elapsed */
    while (LPC_TMR32B1->TCR & 0x01);
  }
  return;
}

/******************************************************************************
** Function name:		TIMER32_0_IRQHandler
**
** Descriptions:		Timer/Counter 0 interrupt handler
**			
**
** parameters:		None
** Returned value:	None
** 
******************************************************************************/
void TIMER32_0_IRQHandler(void)
{
  if ( LPC_TMR32B0->IR & 0x01 )		//MR0 interrupt
  {  
		LPC_TMR32B0->IR = 1;				/* clear interrupt flag */	
		if(MagAMPBuff_Index<SizeofCapTCBuff) {Timer32_1_cap_cnt_tc_en=1;}			//Enable cnt and tc record
		timer32_0_counter++;
	}
			
//	if ( LPC_TMR32B0->IR & (0x01<<1) )			//MR1 interrupt
//  {    
//		LPC_TMR32B0->IR = 2;			/* clear interrupt flag */
//		Timer32_1_cap_cnt_tc0_en=1;				//Enable cnt- and tc-
//		
//		//timer32_1_cap0_tc=LPC_TMR32B1->CR0;	//Store the current timer32_1.CR0
////		timer32_1_cap_tc0=timer32_1_cap_tc;
////		timer32_1_cap_cnt0=timer32_1_cap_cnt; 	//Store the current count of rise edge of timer32_1.cap0
//  }
	
	return;
}

/******************************************************************************
** Function name:		TIMER32_1_IRQHandler
**
** Descriptions:		Timer/Counter 1 interrupt handler
**				executes each 10ms @ 60 MHz CPU Clock
**
** parameters:		None
** Returned value:	None
** 
******************************************************************************/
void TIMER32_1_IRQHandler(void)
{
//  if ( LPC_TMR32B1->IR & 0x01 )				//MR0 interrupt
//  {    
//			LPC_TMR32B1->IR = 1;			/* clear interrupt flag */
//			if(timer32_1_counter<SizeofMagAMPBuff) { MagAMPBuff[timer32_1_counter]=LPC_TMR32B0->TC-TC_Tmp;}		
//			timer32_1_counter++;
//	}
//	
//	if ( LPC_TMR32B1->IR & (0x01<<1) )			//MR1 interrupt
//  {    
//		LPC_TMR32B1->IR = 2;			/* clear interrupt flag */
//					
//		TC_Tmp=LPC_TMR32B0->TC;		//Baseline TC
//  }
	if ( LPC_TMR32B1->IR & (0x01<<4) )				//Capture0 interrupt
  {    
			LPC_TMR32B1->IR = (0x01<<4);			/* clear interrupt flag of cap0 */
			timer32_1_cap_cnt++;
		
			if(Timer32_1_cap_cnt_tc_en)
				{
					Cap_TC_Buff[MagAMPBuff_Index]=LPC_TMR32B1->CR0;			//cap_tc
					Cap_CNT_Buff[MagAMPBuff_Index++]=timer32_1_cap_cnt;		//cap_cnt
				//	timer32_1_cap_cnt=0;																//Reset cap_cnt
					Timer32_1_cap_cnt_tc_en=0;														//Disable
				}
	}
	
	return;
}

/******************************************************************************
** Function name:		enable_timer
**
** Descriptions:		Enable timer
**
** parameters:		timer number: 0 or 1
** Returned value:	None
** 
******************************************************************************/
void enable_timer32(uint8_t timer_num)
{
  if ( timer_num == 0 )
  {
    LPC_TMR32B0->TCR = 1;
  }
  else
  {
    LPC_TMR32B1->TCR = 1;
  }
  return;
}

/******************************************************************************
** Function name:		disable_timer
**
** Descriptions:		Disable timer
**
** parameters:		timer number: 0 or 1
** Returned value:	None
** 
******************************************************************************/
void disable_timer32(uint8_t timer_num)
{
  if ( timer_num == 0 )
  {
    LPC_TMR32B0->TCR = 0;
  }
  else
  {
    LPC_TMR32B1->TCR = 0;
  }
  return;
}

/******************************************************************************
** Function name:		reset_timer
**
** Descriptions:		Reset timer
**
** parameters:		timer number: 0 or 1
** Returned value:	None
** 
******************************************************************************/
void reset_timer32(uint8_t timer_num)
{
  uint32_t regVal;

  if ( timer_num == 0 )
  {
    regVal = LPC_TMR32B0->TCR;
    regVal |= 0x02;
    LPC_TMR32B0->TCR = regVal;
  }
  else
  {
    regVal = LPC_TMR32B1->TCR;
    regVal |= 0x02;
    LPC_TMR32B1->TCR = regVal;
  }
  return;
}

/******************************************************************************
** Function name:		restart_timer
**
** Descriptions:		Re-start timer after reset
**
** parameters:		timer number: 0 or 1
** Returned value:	None
** 
******************************************************************************/
void restart_timer32(uint8_t timer_num)
{
  uint32_t regVal;

  if ( timer_num == 0 )
  {
    regVal = LPC_TMR32B0->TCR;
    regVal &= ~0x02;
    LPC_TMR32B0->TCR = regVal;
  }
  else
  {
    regVal = LPC_TMR32B1->TCR;
    regVal &= ~0x02;
    LPC_TMR32B1->TCR = regVal;
  }
  return;
}

/******************************************************************************
** Function name:		init_timer
**
** Descriptions:		Initialize timer, set timer interval, reset timer,
**				install timer interrupt handler
**
** parameters:		timer number and timer interval
** Returned value:	None
** 
******************************************************************************/
void init_timer32(uint8_t timer_num, uint32_t TimerInterval) 
{
  if ( timer_num == 0 )
  {
    /* Some of the I/O pins need to be clearfully planned if
    you use below module because JTAG and TIMER CAP/MAT pins are muxed. */
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<9);
//    LPC_IOCON->PIO1_5 &= ~0x07;	/*  Timer0_32 I/O config */
//    LPC_IOCON->PIO1_5 |= 0x02;	/* Timer0_32 CAP0 */
//    LPC_IOCON->PIO1_6 &= ~0x07;
//    LPC_IOCON->PIO1_6 |= 0x02;	/* Timer0_32 MAT0 */
//    LPC_IOCON->PIO1_7 &= ~0x07;
//    LPC_IOCON->PIO1_7 |= 0x02;	/* Timer0_32 MAT1 */
//    LPC_IOCON->PIO0_1 &= ~0x07;	
//    LPC_IOCON->PIO0_1 |= 0x02;	/* Timer0_32 MAT2 */
//#ifdef __JTAG_DISABLED
//    LPC_IOCON->R_PIO0_11 &= ~0x07;	
//    LPC_IOCON->R_PIO0_11 |= 0x03;	/* Timer0_32 MAT3 */
//#endif

    timer32_0_counter = 0;
    timer32_0_capture = 0;
    
		LPC_TMR32B0->MCR		&=	~0x07;
		LPC_TMR32B0->MCR		|=	0x03; //MR0 enabled, TC will be reseted on MR0
		
//		LPC_TMR32B0->MCR		&=	~0x38;
//		LPC_TMR32B0->MCR		|=	0x08; //MR1 enabled, TC not reseted and not stopped when matched,  which will be used to hold the time duration for each sampling
//		
				
		LPC_TMR32B0->MR0 = TimerInterval;
//		LPC_TMR32B0->MR1 = SampleDuration;
			
    /* Enable the TIMER0 Interrupt */
    NVIC_EnableIRQ(TIMER_32_0_IRQn);
  }
  else if ( timer_num == 1 )
  {
    /* Some of the I/O pins need to be clearfully planned if
    you use below module because JTAG and TIMER CAP/MAT pins are muxed. */
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<10);
//#ifdef __JTAG_DISABLED
//    LPC_IOCON->R_PIO1_0  &= ~0x07;	/*  Timer1_32 I/O config */
//    LPC_IOCON->R_PIO1_0  |= 0x03;	/* Timer1_32 CAP0 */
//		LPC_IOCON->R_PIO1_0  |= 0x01<<7;	/* digital mode */
		

//#endif
//		LPC_TMR32B1->CTCR		&=	~0xF;
//		LPC_TMR32B1->CTCR		|=	0x01;	//Counter, rise edge, CAP0 selected as the counter input
//		
			LPC_TMR32B1->CCR		|=	0x05;	//CCR.bit2=1, CCR.bit0=1, TC will be copied to CR0 on the rise edge of cap0 and one interrupt will be generated
//		
//		LPC_TMR32B1->MCR		&=	~0x07;
//		LPC_TMR32B1->MCR		|=	0x07; //MR0 enabled, TC reseted and stopped when matched
//		
//		LPC_TMR32B1->MCR		&=	~0x38;
//		LPC_TMR32B1->MCR		|=	0x08; //MR1 enabled, TC not reseted and not stopped when matched,  which will be used to hold the initial value of TC for magnetic counter
		
	
    timer32_1_counter = 0;
    timer32_1_capture = 0;
//    LPC_TMR32B1->MR0 = TimerInterval;
//		LPC_TMR32B1->MR1 = 2;		//Magnatic pulse counter will be started to count after 2 counts

    /* Enable the TIMER1 Interrupt */
    NVIC_EnableIRQ(TIMER_32_1_IRQn);
  }
  return;
}
/******************************************************************************
** Function name:		init_timer32PWM
**
** Descriptions:		Initialize timer as PWM
**
** parameters:		timer number, period and match enable:
**				match_enable[0] = PWM for MAT0 
**				match_enable[1] = PWM for MAT1
**				match_enable[2] = PWM for MAT2
** Returned value:	None
** 
******************************************************************************/
void init_timer32PWM(uint8_t timer_num, uint32_t period, uint8_t match_enable)
{
  disable_timer32(timer_num);
  if (timer_num == 1)
  {
    /* Some of the I/O pins need to be clearfully planned if
    you use below module because JTAG and TIMER CAP/MAT pins are muxed. */
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<10);

    /* Setup the external match register */
    LPC_TMR32B1->EMR = (1<<EMC3)|(1<<EMC2)|(2<<EMC1)|(1<<EMC0)|(1<<3)|(match_enable);

    /* Setup the outputs */
    /* If match0 is enabled, set the output */
    if (match_enable & 0x01)
    {
      LPC_IOCON->R_PIO1_1  &= ~0x07;	
      LPC_IOCON->R_PIO1_1  |= 0x03;		/* Timer1_32 MAT0 */
    }
    /* If match1 is enabled, set the output */
    if (match_enable & 0x02)
    {
      LPC_IOCON->R_PIO1_2 &= ~0x07;
      LPC_IOCON->R_PIO1_2 |= 0x03;		/* Timer1_32 MAT1 */
    }
    /* If match2 is enabled, set the output */
    if (match_enable & 0x04)
    {
      LPC_IOCON->SWDIO_PIO1_3   &= ~0x07;
      LPC_IOCON->SWDIO_PIO1_3   |= 0x03;		/* Timer1_32 MAT2 */
    }
    /* If match3 is enabled, set the output */
    if (match_enable & 0x08)
    {
      LPC_IOCON->PIO1_4           &= ~0x07;
      LPC_IOCON->PIO1_4           |= 0x02;		/* Timer1_32 MAT3 */
    }

    /* Enable the selected PWMs and enable Match3 */
    LPC_TMR32B1->PWMC = (1<<3)|(match_enable);
 
    /* Setup the match registers */
    /* set the period value to a global variable */
    timer32_1_period = period;
    LPC_TMR32B1->MR3 = timer32_1_period;
    LPC_TMR32B1->MR0 = timer32_1_period/2;
    LPC_TMR32B1->MR1 = timer32_1_period/2;
    LPC_TMR32B1->MR2 = timer32_1_period/2;
    LPC_TMR32B1->MCR = 1<<10;				/* Reset on MR3 */
  }
  else
  {
    /* Some of the I/O pins need to be clearfully planned if
    you use below module because JTAG and TIMER CAP/MAT pins are muxed. */
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<9);

    /* Setup the external match register */
    LPC_TMR32B0->EMR = (1<<EMC3)|(2<<EMC2)|(1<<EMC1)|(1<<EMC0)|(1<<0)|(match_enable);
 
		//LPC_TMR32B0->EMR |= match_enable;
		
    /* Setup the outputs */
    /* If match0 is enabled, set the output */
    if (match_enable & 0x01)
    {
//	 	LPC_IOCON->PIO1_6           &= ~0x07;
//	  	LPC_IOCON->PIO1_6           |= 0x02;		/* Timer0_32 MAT0 */
    }
    /* If match1 is enabled, set the output */
    if (match_enable & 0x02)
    {
      LPC_IOCON->PIO1_7           &= ~0x07;
      LPC_IOCON->PIO1_7           |= 0x02;		/* Timer0_32 MAT1 */
    }
    /* If match2 is enabled, set the output */
    if (match_enable & 0x04)
    {
      LPC_IOCON->PIO0_1           &= ~0x07;	
      LPC_IOCON->PIO0_1           |= 0x02;		/* Timer0_32 MAT2 */
    }
    /* If match3 is enabled, set the output */
    if (match_enable & 0x08)
    {
      LPC_IOCON->R_PIO0_11 &= ~0x07;	
      LPC_IOCON->R_PIO0_11 |= 0x03;		/* Timer0_32 MAT3 */
    }

    /* Enable the selected PWMs and enable Match0 */
    LPC_TMR32B0->PWMC = (1<<0)|(match_enable);

    /* Setup the match registers */
    /* set the period value to a global variable */
    timer32_0_period = period;
    LPC_TMR32B0->MR0 = timer32_0_period;
    //LPC_TMR32B0->MR0	= timer32_0_period;	///2;
    //LPC_TMR32B0->MR1	= timer32_0_period/2;
    //LPC_TMR32B0->MR2	= timer32_0_period/2;

    LPC_TMR32B0->MCR = 1<<1;				/* Reset on MR0 */
  }
}

/******************************************************************************
** Function name:		pwm32_setMatch
**
** Descriptions:		Set the pwm32 match values
**
** parameters:		timer number, match numner and the value
**
** Returned value:	None
** 
******************************************************************************/
void setMatch_timer32PWM (uint8_t timer_num, uint8_t match_nr, uint32_t value)
{
  if (timer_num)
  {
    switch (match_nr)
    {
      case 0:
        LPC_TMR32B1->MR0 = value;
      break;
      case 1: 
        LPC_TMR32B1->MR1 = value;
      break;
      case 2:
        LPC_TMR32B1->MR2 = value;
      break;
      case 3: 
        LPC_TMR32B1->MR3 = value;
      break;
      default:
      break;
    }	
  }
  else 
  {
    switch (match_nr)
    {
      case 0:
        LPC_TMR32B0->MR0 = value;
      break;
      case 1: 
        LPC_TMR32B0->MR1 = value;
      break;
      case 2:
        LPC_TMR32B0->MR2 = value;
      break;
      case 3: 
        LPC_TMR32B0->MR3 = value;
      break;
      default:
      break;
    }	
  }
}

/******************************************************************************
**                            End Of File
******************************************************************************/
