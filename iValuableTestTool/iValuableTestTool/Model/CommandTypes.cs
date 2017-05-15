using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Model
{
    public enum CommandTypes
    {
        [Description("系统")]
        System = 0x0000,
        [Description("称重")]
        Weigh = 0x0400,
        [Description("升级")]
        Upgrade = 0x0100,
        [Description("门锁灯")]
        DoorLockLamp = 0x0200,
        [Description("RFID")]
        RFID = 0x0300,
        [Description("CAN")]
        CAN = 0x0600,
        [Description("蓝牙")]
        Bluetooth = 0x0500,
        [Description("货物属性")]
        GoodsAttribute = 0x0A00,
        [Description("显示")]
        Display = 0x0B00,
        [Description("功能通讯")]
        Functions= 0x0F00,
     
    }
}
