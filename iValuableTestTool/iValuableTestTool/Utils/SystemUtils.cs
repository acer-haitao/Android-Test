using DIH.CommunicationProtocol.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class SystemUtils
    {
        private static SystemUtils instance = new SystemUtils();
        public static SystemUtils Instance
        {
            get { return instance; }
        }

        public SystemUtils() { ; }
        public const ushort QueryHeartbeat = (ushort)CommandTypes.System + (ushort)SystemCommand.QueryHeartbeat;
        public const ushort GetError = (ushort)CommandTypes.System + (ushort)SystemCommand.GetError;
        public const ushort QueryDeviceType = (ushort)CommandTypes.System + (ushort)SystemCommand.QueryDeviceType;
        public const ushort QueryMaterialNumber = (ushort)CommandTypes.System + (ushort)SystemCommand.QueryMaterialNumber;
        public const ushort QueryCircuitBoardVersion = (ushort)CommandTypes.System + (ushort)SystemCommand.QueryCircuitBoardVersion;
        public const ushort QuerySoftwareVersion = (ushort)CommandTypes.System + (ushort)SystemCommand.QuerySoftwareVersion;
        public const ushort QueryProtocolVersion = (ushort)CommandTypes.System + (ushort)SystemCommand.QueryProtocolVersion;
        public const ushort QuerySNNumber = (ushort)CommandTypes.System + (ushort)SystemCommand.QuerySNNumber;
        public const ushort SetModeSwitch = (ushort)CommandTypes.System + (ushort)SystemCommand.SetModeSwitch;
        public const ushort SetSystemDateTime = (ushort)CommandTypes.System + (ushort)SystemCommand.SetSystemDateTime;
        public const ushort QuerySystemDateTime = (ushort)CommandTypes.System + (ushort)SystemCommand.QuerySystemDateTime;
        public byte[] SendQueryCommand(ushort CommandType,byte[] bytes)
        {
            return CommunicationTools.Instance.GetSendData(CommandType, bytes);
        }


        public enum SystemCommand
        {
            [Description("错误码")]
            GetError = 0x01,
            [Description("查询设备类型")]
            QueryDeviceType = 0x02,
            [Description("查询电路板物料号")]
            QueryMaterialNumber = 0x03,
            [Description("查询电路板版本号")]
            QueryCircuitBoardVersion = 0x04,
            [Description("查询软件版本号")]
            QuerySoftwareVersion = 0x05,
            [Description("查询通讯协议版本号")]
            QueryProtocolVersion = 0x06,
            [Description("查询电路板SN号")]
            QuerySNNumber = 0x07,
            [Description("查询设备状态（心跳）")]
            QueryHeartbeat = 0x08,
            [Description("设置模式开关")]
            SetModeSwitch = 0x10,
            [Description("设置系统时间")]
            SetSystemDateTime = 0x20,
            [Description("请求系统时间")]
            QuerySystemDateTime = 0x21,
        }
    }
}
