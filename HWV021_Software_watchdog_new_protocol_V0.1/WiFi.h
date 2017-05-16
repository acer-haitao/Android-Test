/****************************************************************************
 *   $Id:: WiFi.h 2015-06-10                  $
 *   Project: MagneticDetector
 *
 *   Description:
 *     This file contains definition and prototype for WiFi configuration.
 *
 *
****************************************************************************/
#ifndef __WIFI_H 
#define __WIFI_H

#include <stdint.h>

#define WIFI_TX_BUFSIZE 	0x20
#define AP								0x0
#define STA								0x01
#define AT								0x0
#define SOCKET						0x01
#define OFF								0x0
#define ON								0x01

#define UDP_HEAD0					'C'
#define UDP_HEAD1					'A'
#define UDP_HEAD2					'C'
#define UDP_HEAD3					'B'

#define POWEROFF					"55"
#define STANDBY						"5a"
#define WORKING						"aa"

#ifdef __cplusplus
extern "C" {
#endif

int SetWiFi_COMM(uint8_t ATorSocket);
int SetWiFiMode(uint8_t);
int WiFiReset_SW(void);
void WiFiReset_HW(void);
int WiFiWModeQuery(void);
int SetCMDEcho(uint8_t on_off);
int WiFiMACQuery(void);
int WiFiIPQuery(void);
int WiFi_LinkQuery(void);
int IP_BCD2BIN(uint8_t *pBuffer_s,uint8_t *pBuffer_d);
void ByteBin2ASIC(uint8_t *pBuffer_s,uint8_t *pBuffer_d,uint8_t B_CNT);
void ByteASIC2Bin(uint8_t *pBuffer_s,uint8_t *pBuffer_d,uint8_t B_CNT);
int REGENA_OFF(void);


#ifdef __cplusplus
}
#endif

#endif /* end __WIFI_H */
/*****************************************************************************
**                            End Of File
******************************************************************************/
