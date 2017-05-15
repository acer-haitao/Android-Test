using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Model
{
    public enum UpgradeCommand
    {
        [Description("查询主控板内的单元板固件类型")]
        QueryFirmwareType = 0x01,
        [Description("查询主控板内的单元板固件版本号")]
        QueryFirmwareVersion = 0x02,
        [Description("查询固件文件属性")]
        QueryFirmwareProperty = 0x03,
        [Description("设置固件文件内容")]
        QueryFirmwareContent = 0x04,
        [Description("设置某单元升级")]
        SetSingleUnitUpgrade = 0x11,
        [Description("设置所有单元升级")]
        SetAllUnitUpgrade = 0x12,
        [Description("上报某单元升级完成")]
        UpgradeState = 0x13,
    }
}
