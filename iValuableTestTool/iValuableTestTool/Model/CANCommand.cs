using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Model
{
    public enum CANCommand
    {
        [Description("设置传感器使能")]
        SetEnable = 0x01,
        [Description("查询传感器使能")]
        QueryEnable = 0x02,
        [Description("设置量程")]
        SetRange = 0x10,
        [Description("查询量程")]
        QueryRange = 0x11,
        [Description("设置安全过载")]
        SetSafeOverload = 0x12,
        [Description("查询安全过载")]
        QuerySafeOverload = 0x13,
        [Description("设置最大过载")]
        SetMaxOverload = 0x14,
        [Description("查询最大过载")]
        QueryMaxOverload = 0x15,
        [Description("设置传感器校准重量")]
        SetSensorCalibrationWeight = 0x20,
        [Description("校准传感器")]
        SensorCalibration = 0x21,
        [Description("设置传感器K值")]
        SetSensorKValue = 0x22,
        [Description("查询K值")]
        QuerySensorKValue = 0x23,
        [Description("查询AD ")]
        QuerySensorADValue = 0x30,
        [Description("清零")]
        SensorCleared = 0x31,
        [Description("查询零值")]
        QueryZeroValue = 0x32,
        [Description("去皮")]
        Peeling = 0x33,
        [Description("查询皮值")]
        QueryPeeling = 0x34,
        [Description("查询传感器重量")]
        QuerySensorWeight = 0x40,
        [Description("查询秤盘重量")]
        QueryPanWeight = 0x41,
        [Description("设置秤盘与传感器对应关系")]
        SetPanAndSensor = 0x50,
        [Description("查询秤盘与传感器对应关系")]
        QueryPanAndSensor = 0x51,
    }
}
