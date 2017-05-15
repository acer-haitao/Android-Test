using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Model
{
    [Serializable]
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet=CharSet.Ansi, Pack = 1)]
    public class InstructionStructure
    {
        [Description("指令头")]
        public ushort CommandHeader { get; set; }
        [Description("目标地址")]
        public ushort DestinationAddress { get; set; }
        [Description("正文长度")]
        public ushort CommandDataLength { get; set; }
        [Description("功能指令")]
        public ushort CommandType { get; set; }
        [Description("指令参数")]
        public byte[] CommandParameter { get; set; }
        [Description("校验码")]
        public ushort CommandCheckNumber { get; set; }
    }
}
