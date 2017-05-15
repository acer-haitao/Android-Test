/****************************************************************************
 *   $Id:: timer16.c 3635 2010-06-02 00:31:46Z usb00423    (CuiDH modified based on timer32.c)    $
 *   Project: NXP LPC11xx 32-bit timer example
 *
 *   Description:
 *     This file contains 16-bit timer code example which include timer 
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
#include "timer16.h"

volatile uint32_t timer16_0_counter = 0;
volatile uint32_t timer16_1_counter = 0;
volatile uint32_t timer16_0_capture = 0;
volatile uint32_t timer16_1_capture = 0;
volatile uint32_t timer16_0_period = 0;
volatile uint32_t timer16_1_period = 0;

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
void delay16Ms(uint8_t timer_num, uint32_t delayInMs)
{
  if (timer_num == 0)
  {
    /* setup timer #0 for delay */
    LPC_TMR16B0->TCR = 0x02;		/* reset timer */
		LPC_TMR16B0->PR  = (delayInMs * (SystemCoreClock / 1000))&0xFFFF;		/* set prescaler*/
	  LPC_TMR16B0->MR0 = ((delayInMs * (SystemCoreClock / 1000))>>16)&0xFFFF;
    LPC_TMR16B0->IR  = 0xff;		/* reset all interrrupts */
		LPC_TMR16B0->MCR = 0x04;		/* stop timer on match */
	  LPC_TMR16B0->TCR = 0x01;		/* start timer */
  
    /* wait until delay time has elapsed */
		while (LPC_TMR16B0->TCR & 0x01);
  }
  else if (timer_num == 1)
  {
    /* setup timer #1 for delay */
    LPC_TMR16B1->TCR = 0x02;		/* reset timer */
    LPC_TMR16B1->PR  = (delayInMs * (SystemCoreClock / 1000))&0xFFFF;		/* set prescaler*/
    LPC_TMR16B1->MR0 = ((delayInMs * (SystemCoreClock / 1000))>>16)&0xFFFF;
    LPC_TMR16B1->IR  = 0xff;		/* reset all interrrupts */
    LPC_TMR16B1->MCR = 0x04;		/* stop timer on match */
    LPC_TMR16B1->TCR = 0x01;		/* start timer */
  
    /* wait until delay time has elapsed */
    while (LPC_TMR16B1->TCR & 0x01);
  }
  return;
}

/******************************************************************************
** Function name:		TIMER16_0_IRQHandler
**
** Descriptions:		Timer/Counter 0 interrupt handler
**
** parameters:		None
** Returned value:	None
** 
******************************************************************************/
void TIMER16_0_IRQHandler(void)
{
  if ( LPC_TMR16B0->IR & 0x01 )
  {  
		LPC_TMR16B0->IR = 1;								/* clear interrupt flag */		
	}
  return;
}

/******************************************************************************
** Function name:		TIMER16_1_IRQHandler
**
** Descriptions:		Timer/Counter 1 interrupt handler
**
** parameters:		None
** Returned value:	None
** 
******************************************************************************/
void TIMER16_1_IRQHandler(void)
{
  if ( LPC_TMR16B1->IR & 0x01 )
  {    
	LPC_TMR16B1->IR = 1;			/* clear interrupt flag */
/*add the IRQ task below*/
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
void enable_timer16(uint8_t timer_num)
{
  if ( timer_num == 0 )
  {
    LPC_TMR16B0->TCR = 1;
  }
  else
  {
    LPC_TMR16B1->TCR = 1;
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
void disable_timer16(uint8_t timer_num)
{
  if ( timer_num == 0 )
  {
    LPC_TMR16B0->TCR = 0;
  }
  else
  {
    LPC_TMR16B1->TCR = 0;
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
void reset_timer16(uint8_t timer_num)
{
  uint32_t regVal;

  if ( timer_num == 0 )
  {
    regVal = LPC_TMR16B0->TCR;
    regVal |= 0x02;
    LPC_TMR16B0->TCR = regVal;
  }
  else
  {
    regVal = LPC_TMR16B1->TCR;
    regVal |= 0x02;
    LPC_TMR16B1->TCR = regVal;
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
void init_timer16(uint8_t timer_num, uint32_t TimerInterval) 
{
  if ( timer_num == 0 )
  {
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<7);		//Enables clock for 16-bit counter/timer0
		
		timer16_0_counter = 0;
    timer16_0_capture = 0;
		LPC_TMR16B0->MCR = 0x04;		/* stop timer on match */
    
		//LPC_TMR16B0->MCR		&=	~0x07;
		//LPC_TMR16B0->MCR		|=	0x03; //MR0 enabled, TC will be reseted on MR0
				
		LPC_TMR16B0->MR0 = TimerInterval;

		/* Enable the TIMER0 Interrupt */
    //NVIC_EnableIRQ(TIMER_32_0_IRQn);
  }
  else if ( timer_num == 1 )
  {
    /* Some of the I/O pins need to be clearfully planned if
    you use below module because JTAG and TIMER CAP/MAT pins are muxed. */
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<8);		//Enables clock for 16-bit counter/timer1
		reset_timer16(1);												//Reset timer16_1

    /*Config time16_1 match outputs as PWM output:MAT0 and MAT1*/
		LPC_TMR16B1->PWMC	|=	(1<<0);		//PWM mode is anabled for CT16B1_MAT0
		LPC_TMR16B1->PWMC	|=	(1<<1);		//PWM mode is anabled for CT16B1_MAT1
		
		/*Config what operations are performs when the match register matchs the Timer counter
		CT16B1_MAT0: positive SPWM output, do nothing when matched
		CT16B1_MAT1: negtive SPWM output, do nothing when matched
		CT16B1_MAT2: Cycle period time of SPWM, interrupt will be generated when matched, TC will be reset when matched
				*/
		LPC_TMR16B1->MCR	&=	~0x1C0;		
		LPC_TMR16B1->MCR	|=	0xC0;		//Interrupt will be generated and timer counter will be reset when timer16_1 MR2 matches the value in the TC
		
		timer16_1_counter = 0;
    timer16_1_capture = 0;
    LPC_TMR16B1->MR2 = TimerInterval;			//SPWM cycle
		//LPC_TMR16B1->MR0 = TimerInterval-*SPWM1_sample_ptr_base;	  //*SPWM_Sample_Table stores the SPWM samples period in CPU clock of 48MHz.
		LPC_TMR16B1->MR1 = TimerInterval+1;		//Output LOW during phase 0

    /* Enable the TIMER1 Interrupt */
    //NVIC_EnableIRQ(TIMER_16_1_IRQn);
  }
  return;
}
/******************************************************************************
** Function name:		init_timer16PWM
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
void init_timer16PWM(uint8_t timer_num, uint32_t period, uint8_t match_enable)
{
  disable_timer16(timer_num);
  if (timer_num == 1)
  {
    /* Some of the I/O pins need to be clearfully planned if
    you use below module because JTAG and TIMER CAP/MAT pins are muxed. */
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<10);

    /* Setup the external match register */
    LPC_TMR16B1->EMR = (1<<EMC3)|(1<<EMC2)|(2<<EMC1)|(1<<EMC0)|(1<<3)|(match_enable);

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
    LPC_TMR16B1->PWMC = (1<<3)|(match_enable);
 
    /* Setup the match registers */
    /* set the period value to a global variable */
    timer16_1_period = period;
    LPC_TMR16B1->MR3 = timer16_1_period;
    LPC_TMR16B1->MR0 = timer16_1_period/2;
    LPC_TMR16B1->MR1 = timer16_1_period/2;
    LPC_TMR16B1->MR2 = timer16_1_period/2;
    LPC_TMR16B1->MCR = 1<<10;				/* Reset on MR3 */
  }
  else
  {
    /* Some of the I/O pins need to be clearfully planned if
    you use below module because JTAG and TIMER CAP/MAT pins are muxed. */
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<9);

    /* Setup the external match register */
    LPC_TMR16B0->EMR = (1<<EMC3)|(2<<EMC2)|(1<<EMC1)|(1<<EMC0)|(1<<0)|(match_enable);
 
		//LPC_TMR16B0->EMR |= match_enable;
		
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
    LPC_TMR16B0->PWMC = (1<<0)|(match_enable);

    /* Setup the match registers */
    /* set the period value to a global variable */
    timer16_0_period = period;
    LPC_TMR16B0->MR0 = timer16_0_period;
    //LPC_TMR16B0->MR0	= timer16_0_period;	///2;
    //LPC_TMR16B0->MR1	= timer16_0_period/2;
    //LPC_TMR16B0->MR2	= timer16_0_period/2;

    LPC_TMR16B0->MCR = 1<<1;				/* Reset on MR0 */
  }
}

/******************************************************************************
** Function name:		pwm16_setMatch
**
** Descriptions:		Set the pwm32 match values
**
** parameters:		timer number, match numner and the value
**
** Returned value:	None
** 
******************************************************************************/
void setMatch_timer16PWM (uint8_t timer_num, uint8_t match_nr, uint32_t value)
{
  if (timer_num)
  {
    switch (match_nr)
    {
      case 0:
        LPC_TMR16B1->MR0 = value;
      break;
      case 1: 
        LPC_TMR16B1->MR1 = value;
      break;
      case 2:
        LPC_TMR16B1->MR2 = value;
      break;
      case 3: 
        LPC_TMR16B1->MR3 = value;
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
        LPC_TMR16B0->MR0 = value;
      break;
      case 1: 
        LPC_TMR16B0->MR1 = value;
      break;
      case 2:
        LPC_TMR16B0->MR2 = value;
      break;
      case 3: 
        LPC_TMR16B0->MR3 = value;
      break;
      default:
      break;
    }	
  }
}

/******************************************************************************
**                            End Of File
******************************************************************************/
