#include <LPC11xx.h>
#include "gpio.h"

volatile uint32_t gpio0_counter = 0;
volatile uint32_t gpio1_counter = 0;
volatile uint32_t gpio2_counter = 0;
volatile uint32_t gpio3_counter = 0;
volatile uint32_t p0_1_counter  = 0;
volatile uint32_t p1_1_counter  = 0;
volatile uint32_t p2_1_counter  = 0;
volatile uint32_t p3_1_counter  = 0;

/*****************************************************************************
** Function name:		GPIOInit
**
** Descriptions:		Initialize GPIO, install the
**						GPIO interrupt handler
**
** parameters:			None
** Returned value:		true or false, return false if the VIC table
**						is full and GPIO interrupt handler can be
**						installed.
** 
*****************************************************************************/
void GPIOInit( void )
{
	//int i;
  /* Enable AHB clock to the GPIO domain. */
  LPC_SYSCON->SYSAHBCLKCTRL |= (1<<6);

//LED indicator
	GPIOSetDir(PORT0,8,E_IO_OUTPUT);				//BLUE, 0--ON, 1--OFF
	GPIOSetValue(PORT0,8,1);	
	
//WiFi module		
	GPIOSetDir(PORT0,3,E_IO_OUTPUT);				//0--WiFi reset, 1--Release WiFi reset
	GPIOSetValue(PORT0,3,1);	
	
	GPIOSetDir(PORT0,9,E_IO_INPUT);				 //0--WiFi module ready, 1--WiFi module in in configuration process
	
	GPIOSetDir(PORT1,1,E_IO_INPUT);					//0--WiFi module linked successfully
	
	GPIOSetDir(PORT1,2,E_IO_INPUT);					//0--Request WiFi to be configured as AP mode, 1--set WiFi module to Normal mode

//Magnetic pulse input	
	LPC_IOCON->R_PIO1_0  &= ~0x07;	//PIO1_0 as CT32B1_CAP0
  LPC_IOCON->R_PIO1_0  |= 0x03;
	LPC_IOCON->R_PIO1_0  |= 0x01<<7;	/* digital mode */
		
/*	
	//IM0
	GPIOSetDir(PORT1,0,E_IO_OUTPUT);
	GPIOSetValue(PORT1,0,1);
	
	//IM1
	GPIOSetDir(PORT1,1,E_IO_OUTPUT);
	GPIOSetValue(PORT1,1,1);
	
	//IM2
	GPIOSetDir(PORT1,2,E_IO_OUTPUT);
	GPIOSetValue(PORT1,2,1);
	
	//DISP_EN
	LPC_IOCON->R_PIO0_11  &= ~0x07;
  LPC_IOCON->R_PIO0_11  |= 0x01;
	GPIOSetDir( PORT0, 11, E_IO_OUTPUT);
	GPIOSetValue( PORT0, 11, 1);
	
	//FLASH_CS
	LPC_IOCON->PIO2_0 &= ~0x07;
	GPIOSetDir( PORT2, 0, E_IO_OUTPUT);
	GPIOSetValue( PORT2, 0, 1);
*/

  /* Set up NVIC when I/O pins are configured as external interrupts. */
  //NVIC_EnableIRQ(EINT0_IRQn);
  //NVIC_EnableIRQ(EINT1_IRQn);
  //NVIC_EnableIRQ(EINT2_IRQn);
  //NVIC_EnableIRQ(EINT3_IRQn);
  return;
}

/*****************************************************************************
** Function name:		GPIOSetInterrupt
**
** Descriptions:		Set interrupt sense, event, etc.
**						edge or level, 0 is edge, 1 is level
**						single or double edge, 0 is single, 1 is double 
**						active high or low, etc.
**
** parameters:			port num, bit position, sense, single/doube, polarity
** Returned value:		None
** 
*****************************************************************************/
void GPIOSetInterrupt( uint32_t portNum, uint32_t bitPosi, uint32_t sense,
			uint32_t single, uint32_t event )
{
  switch ( portNum )
  {
	case PORT0:
	  if ( sense == 0 )
	  {
		LPC_GPIO0->IS &= ~(0x1<<bitPosi);
		/* single or double only applies when sense is 0(edge trigger). */
		if ( single == 0 )
		  LPC_GPIO0->IBE &= ~(0x1<<bitPosi);
		else
		  LPC_GPIO0->IBE |= (0x1<<bitPosi);
	  }
	  else
	  	LPC_GPIO0->IS |= (0x1<<bitPosi);
	  if ( event == 0 )
		LPC_GPIO0->IEV &= ~(0x1<<bitPosi);
	  else
		LPC_GPIO0->IEV |= (0x1<<bitPosi);
	break;
 	case PORT1:
	  if ( sense == 0 )
	  {
		LPC_GPIO1->IS &= ~(0x1<<bitPosi);
		/* single or double only applies when sense is 0(edge trigger). */
		if ( single == 0 )
		  LPC_GPIO1->IBE &= ~(0x1<<bitPosi);
		else
		  LPC_GPIO1->IBE |= (0x1<<bitPosi);
	  }
	  else
	  	LPC_GPIO1->IS |= (0x1<<bitPosi);
	  if ( event == 0 )
		LPC_GPIO1->IEV &= ~(0x1<<bitPosi);
	  else
		LPC_GPIO1->IEV |= (0x1<<bitPosi);  
	break;
	case PORT2:
	  if ( sense == 0 )
	  {
		LPC_GPIO2->IS &= ~(0x1<<bitPosi);
		/* single or double only applies when sense is 0(edge trigger). */
		if ( single == 0 )
		  LPC_GPIO2->IBE &= ~(0x1<<bitPosi);
		else
		  LPC_GPIO2->IBE |= (0x1<<bitPosi);
	  }
	  else
	  	LPC_GPIO2->IS |= (0x1<<bitPosi);
	  if ( event == 0 )
		LPC_GPIO2->IEV &= ~(0x1<<bitPosi);
	  else
		LPC_GPIO2->IEV |= (0x1<<bitPosi);  
	break;
	case PORT3:
	  if ( sense == 0 )
	  {
			LPC_GPIO3->IS &= ~(0x1<<bitPosi);
			/* single or double only applies when sense is 0(edge trigger). */
			if ( single == 0 )
				LPC_GPIO3->IBE &= ~(0x1<<bitPosi);
			else
				LPC_GPIO3->IBE |= (0x1<<bitPosi);
	  }
	  else
	  	LPC_GPIO3->IS |= (0x1<<bitPosi);
	  if ( event == 0 )
			LPC_GPIO3->IEV &= ~(0x1<<bitPosi);
	  else
			LPC_GPIO3->IEV |= (0x1<<bitPosi);	  
	break;
	default:
	  break;
  }
  return;
}

/*****************************************************************************
** Function name:		GPIOIntEnable
**
** Descriptions:		Enable Interrupt Mask for a port pin.
**
** parameters:			port num, bit position
** Returned value:		None
** 
*****************************************************************************/
void GPIOIntEnable( uint32_t portNum, uint32_t bitPosi )
{
  switch ( portNum )
  {
	case PORT0:
	  LPC_GPIO0->IE |= (0x1<<bitPosi); 
	break;
 	case PORT1:
	  LPC_GPIO1->IE |= (0x1<<bitPosi);	
	break;
	case PORT2:
	  LPC_GPIO2->IE |= (0x1<<bitPosi);	    
	break;
	case PORT3:
	  LPC_GPIO3->IE |= (0x1<<bitPosi);	    
	break;
	default:
	  break;
  }
  return;
}

/*****************************************************************************
** Function name:		GPIOIntDisable
**
** Descriptions:		Disable Interrupt Mask for a port pin.
**
** parameters:			port num, bit position
** Returned value:		None
** 
*****************************************************************************/
void GPIOIntDisable( uint32_t portNum, uint32_t bitPosi )
{
  switch ( portNum )
  {
	case PORT0:
	  LPC_GPIO0->IE &= ~(0x1<<bitPosi); 
	break;
 	case PORT1:
	  LPC_GPIO1->IE &= ~(0x1<<bitPosi);	
	break;
	case PORT2:
	  LPC_GPIO2->IE &= ~(0x1<<bitPosi);	    
	break;
	case PORT3:
	  LPC_GPIO3->IE &= ~(0x1<<bitPosi);	    
	break;
	default:
	  break;
  }
  return;
}

/*****************************************************************************
** Function name:		GPIOIntStatus
**
** Descriptions:		Get Interrupt status for a port pin.
**
** parameters:			port num, bit position
** Returned value:		None
** 
*****************************************************************************/
uint32_t GPIOIntStatus( uint32_t portNum, uint32_t bitPosi )
{
  uint32_t regVal = 0;

  switch ( portNum )
  {
	case PORT0:
	  if ( LPC_GPIO0->MIS & (0x1<<bitPosi) )
		regVal = 1;
	break;
 	case PORT1:
	  if ( LPC_GPIO1->MIS & (0x1<<bitPosi) )
		regVal = 1;	
	break;
	case PORT2:
	  if ( LPC_GPIO2->MIS & (0x1<<bitPosi) )
		regVal = 1;		    
	break;
	case PORT3:
	  if ( LPC_GPIO3->MIS & (0x1<<bitPosi) )
		regVal = 1;		    
	break;
	default:
	  break;
  }
  return ( regVal );
}

/*****************************************************************************
** Function name:		GPIOIntClear
**
** Descriptions:		Clear Interrupt for a port pin.
**
** parameters:			port num, bit position
** Returned value:		None
** 
*****************************************************************************/
void GPIOIntClear( uint32_t portNum, uint32_t bitPosi )
{
  switch ( portNum )
  {
	case PORT0:
	  LPC_GPIO0->IC |= (0x1<<bitPosi); 
	break;
 	case PORT1:
	  LPC_GPIO1->IC |= (0x1<<bitPosi);	
	break;
	case PORT2:
	  LPC_GPIO2->IC |= (0x1<<bitPosi);	    
	break;
	case PORT3:
	  LPC_GPIO3->IC |= (0x1<<bitPosi);	    
	break;
	default:
	  break;
  }
  return;
}

/*****************************************************************************
** Function name:		GPIOGetValue
**
** Descriptions:		Get a bitvalue in a specific bit position
**						in GPIO portX(X is the port number.)
**
** parameters:			port num, bit position
** Returned value:		None
**
*****************************************************************************/
uint32_t GPIOGetValue( uint32_t portNum, uint32_t bitPosi )
{
  return LPC_GPIO[portNum]->MASKED_ACCESS[(1<<bitPosi)];
  //return outpw(0,0);
}
/*****************************************************************************
** Function name:		GPIOSetValue
**
** Descriptions:		Set/clear a bitvalue in a specific bit position
**						in GPIO portX(X is the port number.)
**
** parameters:			port num, bit position, bit value
** Returned value:		None
**
*****************************************************************************/
void GPIOSetValue( uint32_t portNum, uint32_t bitPosi, uint32_t bitVal )
{
  LPC_GPIO[portNum]->MASKED_ACCESS[(1<<bitPosi)] = (bitVal<<bitPosi);
}
/*****************************************************************************
** Function name:		GPIOSetDir
**
** Descriptions:		Set the direction in GPIO port
**
** parameters:			port num, bit position, direction (1 out, 0 input)
** Returned value:		None
**
*****************************************************************************/
void GPIOSetDir( uint32_t portNum, uint32_t bitPosi, uint32_t dir )
{
  if(dir)
	LPC_GPIO[portNum]->DIR |= 1<<bitPosi;
  else
	LPC_GPIO[portNum]->DIR &= ~(1<<bitPosi);
}

// Define interface
// Make it high
void DrvGPIO_SetBit( uint32_t portNum, uint32_t bitPosi )
{
	// Make out put
	//GPIOSetDir(	portNum, bitPosi ,E_IO_OUTPUT);
	// Set value
	GPIOSetValue( portNum, bitPosi,1);

}
// Make it low
void DrvGPIO_ClrBit( uint32_t portNum, uint32_t bitPosi )
{
	// Make out put
	//GPIOSetDir(	portNum, bitPosi ,E_IO_OUTPUT);
	// Set value
	GPIOSetValue( portNum, bitPosi,0);

}
// Get value
uint32_t DrvGPIO_GetBit( uint32_t portNum, uint32_t bitPosi )
{
	// Set value
	return GPIOGetValue( portNum, bitPosi);

}
void DrvGPIO_Open( uint32_t portNum, uint32_t bitPosi, uint32_t dir )
{
	 
	 if (dir== E_IO_OUTPUT)	//1
	 {
	 	
		if ((LPC_GPIO[portNum]->DIR & (1<<bitPosi)) ==0) // current input
		{
			if (GPIOGetValue(portNum,bitPosi)==0)	// low
			{
				GPIOSetValue( portNum, bitPosi,0);	
			}
			else   
			{
				GPIOSetValue( portNum, bitPosi,1); // high
			}
		}
		
	 }
	 GPIOSetDir(portNum,bitPosi,dir);
	 
}



/******************************************************************************
**                            End Of File
******************************************************************************/
