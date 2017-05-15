using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Model
{
    public enum RFIDCommand
    {
        [Description("查询RFID 频段")]
        QueryRFIDFrequencies = 0x01,
        [Description("查询协议")]
        QueryRFIDProtocol = 0x02,
        [Description("查询RFID卡容量")]
        QueryRFIDCapacity = 0x03,
        [Description("查询 RFID 物理ID")]
        QueryRFIDPhysicsId = 0x10,
        [Description("卡片移入，请求读卡")]
        ResponseReadRFIDCard = 0x11,
        [Description("卡片移除，请求读卡")]
        ResponseReMoveRFIDCard = 0x12,
        [Description("查询RFID内容")]
        QueryRFIDContent = 0x20,
        [Description("设置RFID 内容")]
        SetRFIDContent = 0x21,
    }
}
