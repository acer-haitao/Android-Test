using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Model
{
    public enum DoorLockLampCommand
    {
        [Description("设置锁类型")]
        SetLockType = 0x01,
        [Description("查询锁类型")]
        QueryLockType = 0x02,
        [Description("开锁")]
        SetUnlock = 0x10,
        [Description("开照明灯")]
        SetDrivinglights = 0x11,
        [Description("开报警灯")]
        SetOpenWarninglamp = 0x12,
        [Description("开报警蜂鸣器")]
        SetOpenAlarmBuzzer = 0x13,
        [Description("查询锁状态")]
        QueryLockState = 0x20,
        [Description("查询门状态")]
        QueryDoorState = 0x21,
        [Description("查询照明灯状态")]
        QueryLightState = 0x22,
        [Description("查询报警灯状态")]
        QueryWarningLightState = 0x23,
        [Description("查询报警蜂鸣器状态")]
        QueryWarningBuzzerState = 0x24,
        [Description("上报锁状态 ")]
        ReturnLockState = 0x30,
        [Description("上报门状态")]
        ReturnDoorState = 0x31,
    }
}
