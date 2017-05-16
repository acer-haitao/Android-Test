using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace iValuableTestCommonCode
{
    [Serializable]
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
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


    public class Serializer
    {
        private static Serializer instance = new Serializer();
        public static Serializer Instance
        {
            get { return instance; }
        }
        public ushort Header = 0xAA55;
        public byte[] Serialize(ushort uDestinationAddress, ushort commandType, byte[] commandParamter)
        {
            byte[] bCommandType = BitConverter.GetBytes(commandType);
            List<byte> bytes = new List<byte>();
            bytes.AddRange(bCommandType);
            bytes.AddRange(commandParamter);
            ushort uCommandDataLength = (ushort)(2 + commandParamter.Length);
            ushort checkNumber = (ushort)GenerateCheckSum(bytes.ToArray());
            InstructionStructure instruction = new InstructionStructure()
            {
                CommandHeader = Header,
                DestinationAddress = uDestinationAddress,
                CommandDataLength = uCommandDataLength,
                CommandType = commandType,
                CommandParameter = commandParamter,
                CommandCheckNumber = checkNumber
            };

            return ConvertToBytesByStruct(instruction);
        }
        public byte[] ConvertToBytesByStruct(InstructionStructure instruction)
        {
            List<byte> finalBytes = new List<byte>();
            finalBytes.AddRange(BitConverter.GetBytes(instruction.CommandHeader));
            finalBytes.AddRange(BitConverter.GetBytes(instruction.DestinationAddress));
            finalBytes.AddRange(BitConverter.GetBytes(instruction.CommandDataLength));
            finalBytes.AddRange(BitConverter.GetBytes(instruction.CommandType));
            finalBytes.AddRange(instruction.CommandParameter);
            finalBytes.AddRange(BitConverter.GetBytes(instruction.CommandCheckNumber));
            return finalBytes.ToArray();
        }
        public InstructionStructure Deserialize(byte[] commandData)
        {
            InstructionStructure instruction = new InstructionStructure()
            {
                CommandHeader = BitConverter.ToUInt16(commandData.Skip(0).Take(2).ToArray(), 0),
                DestinationAddress = BitConverter.ToUInt16(commandData.Skip(2).Take(2).ToArray(), 0),
                CommandDataLength = BitConverter.ToUInt16(commandData.Skip(4).Take(2).ToArray(), 0),
                CommandType = BitConverter.ToUInt16(commandData.Skip(6).Take(2).ToArray(), 0),
                CommandParameter = commandData.Skip(8).Take(commandData.Length - 2 - 8).ToArray(),
                CommandCheckNumber = BitConverter.ToUInt16(commandData.Skip(commandData.Length - 2).Take(2).ToArray(), 0)
            };
            return instruction;
        }
        public ushort GenerateCheckSum(byte[] data)
        {
            int start = 0;
            int length = data.Length;
            const ushort poly = 0x1021;
            ushort crc = 0;
            while (length-- > 0)
            {
                byte bt = data[start++];
                for (int i = 0; i < 8; i++)
                {
                    bool b1 = (crc & 0x8000U) != 0;
                    bool b2 = (bt & 0x80U) != 0;
                    if (b1 != b2) crc = (ushort)((crc << 1) ^ poly);
                    else crc <<= 1;
                    bt <<= 1;
                }
            }
            return crc;
        }
    }
    public class TransmitCmd
    {
        private static TransmitCmd instance = new TransmitCmd();
        public static TransmitCmd Instance
        {
            get { return instance; }
        }
        public TransmitCmd() { ; }


        public enum CommandTypes
        {
            [Description("系统")]
            System = 0x0000,
            [Description("升级")]
            Upgrade = 0x0100,
            [Description("门锁灯")]
            DoorKockLamp = 0x0200,
            [Description("RFID")]
            RFID = 0x0300,
            [Description("称重")]
            Weigh = 0x0400,
            [Description("蓝牙")]
            Bluetooth = 0x0500,
            [Description("CAN")]
            CAN = 0x0600,
            [Description("以太网")]
            LAN = 0x0700,
            [Description("货物属性")]
            Goods = 0x0A00,
            [Description("显示")]
            Display = 0x0B00,
            [Description("LED")]
            LED = 0x0C00,
            [Description("功能通讯")]
            Functions = 0x0F00,

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
            [Description("设置系统模式")]
            SetModeSwitch = 0x10,
            [Description("擦除系统设置")]
            SetEraseSetting = 0x11,
            [Description("查询铁电状态")]
            QueryNvramState = 0x12,
            [Description("查询系统设置版本号")]
            QuerySettingVersion = 0x13,
            [Description("查询拨码ID")]
            QueryID = 0x21,
            [Description("上报拨码ID")]
            ReportID = 0x22,
        }

        public byte[] QueryDeviceType(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QueryDeviceType, new byte[1] { 0 });
            return sendData;
        }
        public byte[] QueryMaterialNumber(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QueryMaterialNumber, new byte[1] { 0 });
            return sendData;
        }
        public byte[] QueryCircuitBoardVersion(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QueryCircuitBoardVersion, new byte[1] { 0 });
            return sendData;
        }
        public byte[] QuerySoftwareVersion(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QuerySoftwareVersion, new byte[1] { 0 });
            return sendData;
        }
        public byte[] QueryProtocolVersion(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QueryProtocolVersion, new byte[1] { 0 });
            return sendData;
        }
        public byte[] QuerySNNumber(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QuerySNNumber, new byte[1] { 0 });
            return sendData;
        }
        public byte[] QueryHeartbeat(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QueryHeartbeat, new byte[1] { 0 });
            return sendData;
        }
        public byte[] SetModeSwitch(ushort uDestinationAddress, byte data)
        {
            byte[] Paramter = new byte[] { data };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.SetModeSwitch, Paramter);
            return sendData;
        }
        public byte[] SetEraseSetting(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.SetEraseSetting, new byte[1] { 0 });
            return sendData;
        }
        public byte[] QueryNvramState(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QueryNvramState, new byte[1] { 0 });
            return sendData;
        }
        public byte[] QuerySettingVersion(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QuerySettingVersion, new byte[1] { 0 });
            return sendData;
        }       
         public byte[] QueryID(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.System + (ushort)SystemCommand.QueryID, new byte[1] { 0 });
            return sendData;
        }
        public enum UpgradeCommand
        {
            [Description("进入升级模式")]
            UpgradeToBoot = 0x01,
            [Description("进入正常模式")]
            UpgradeToNormal = 0x02,
            [Description("读唯一标识")]
            UpgradeReadID = 0x11,
            [Description("扇区查空")]
            UpgradeCheckSector = 0x12,
            [Description("擦除扇区")]
            UpgradeEraseSector = 0x13,
            [Description("擦除全部区域")]
            UpgradeErase = 0x21,
            [Description("设置每页内容")]
            UpgradeWritePage = 0x22,
        }
        public byte[] UpgradeToBoot(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Upgrade + (ushort)UpgradeCommand.UpgradeToBoot, new byte[1] { 0 });
            return sendData;
        }
        public byte[] UpgradeToNormal(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Upgrade + (ushort)UpgradeCommand.UpgradeToNormal, new byte[1] { 0 });
            return sendData;
        }
        public byte[] UpgradeReadID(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Upgrade + (ushort)UpgradeCommand.UpgradeReadID, new byte[1] { 0 });
            return sendData;
        }
        public byte[] UpgradeCheckSector(ushort uDestinationAddress, byte start, byte end)
        {
            byte[] Paramter = new byte[] { start, end };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Upgrade + (ushort)UpgradeCommand.UpgradeCheckSector, Paramter);
            return sendData;
        }
        public byte[] UpgradeEraseSector(ushort uDestinationAddress, byte start, byte end)
        {
            byte[] Paramter = new byte[] { start, end };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Upgrade + (ushort)UpgradeCommand.UpgradeCheckSector, Paramter);
            return sendData;
        }
        public byte[] UpgradeErase(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Upgrade + (ushort)UpgradeCommand.UpgradeErase, new byte[1] { 0 });
            return sendData;
        }
        public byte[] UpgradeWritePage(ushort uDestinationAddress, ushort page, byte[] data)
        {
            byte[] Paramter = new byte[0x82];
            Array.Copy(BitConverter.GetBytes(page), 0, Paramter, 0, 2);
            Array.Copy(data, 0, Paramter, 2, 0x80);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Upgrade + (ushort)UpgradeCommand.UpgradeWritePage, Paramter);
            return sendData;
        }
 
        public enum RfidCommand
        {
            [Description("查询RFID 频段")]
            RfidQueryBand = 0x01,
            [Description("查询协议")]
            RfidQueryProtocol = 0x02,
            [Description("查询 RFID 物理ID")]
            RfidQueryID = 0x10,
            [Description("上报RFID移入")]
            RfidReportIn = 0x11,
            [Description("上报RFID移出")]
            RfidReportOut = 0x12,
            [Description("查询RFID容量")]
            RfidQueryCapacity = 0x13,
            [Description("设置RFID擦除")]
            RfidSetErase = 0x14,
            [Description("查询RFID内容")]
            RfidQueryData = 0x20,
            [Description("设置RFID 内容")]
            RfidSetdata = 0x21,
            [Description("设置判断卡移出的读卡次数")]
            RfidSetReadCardCount = 0x30,
            [Description("查询判断卡移出的读卡次数")]
            RfidGetReadCardCount = 0x31,

        }
        public byte[] RfidQueryBand(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidQueryBand, new byte[1] { 0 });
            return sendData;
        }
        public byte[] RfidQueryProtocol(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidQueryProtocol, new byte[1] { 0 });
            return sendData;
        }
        public byte[] RfidQueryID(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidQueryID, new byte[1] { 0 });
            return sendData;
        }
        public byte[] RfidQueryCapacity(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidQueryCapacity, new byte[1] { 0 });
            return sendData;
        }
        public byte[] RfidSetErase(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidSetErase, new byte[1] { 0 });
            return sendData;
        }
        public byte[] RfidQueryData(ushort uDestinationAddress,UInt16 Offset,UInt16 Length)
        {
            byte[] Paramter = new byte[4];
            Array.Copy(BitConverter.GetBytes(Offset), 0, Paramter, 0, 2);
            Array.Copy(BitConverter.GetBytes(Length), 0, Paramter, 2, 2);            
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidQueryData,Paramter);
            return sendData;
        }
        public byte[] RfidSetdata(ushort uDestinationAddress, UInt16 Offset,byte[] data,int Length)
        {
            byte[] Paramter = new byte[data.Length + 2];
            Array.Copy(BitConverter.GetBytes(Offset), 0, Paramter, 0, 2);
            Array.Copy(data, 0, Paramter, 2, Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidSetdata, Paramter);
            return sendData;
        }
        public byte[] RfidGetReadCardCount(ushort uDestinationAddress)
        {
            byte[] Paramter = new byte[]{0};
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidGetReadCardCount, Paramter);
            return sendData;
        }
        public byte[] RfidSetReadCardCount(ushort uDestinationAddress, byte data)
        {
            byte[] Paramter = new byte[] { data };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.RFID + (ushort)RfidCommand.RfidSetReadCardCount, Paramter);
            return sendData;
        }
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        [Serializable()]
        public struct ZYFStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] CardHead;
            public UInt16 Length;
            public UInt16 Checkout;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Version;

            public byte Type;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] ID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 92)]
            public byte[] Contents;          
        };

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        [Serializable()]
        public struct ChargeStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] CardHead;
            public UInt16 Length;
            public UInt16 Checkout;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Version;

            public byte Reserve;
            
            public UInt64 Password;
        };
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        [Serializable()]
        public struct NarcoticMaterialStruct
        {
	        public UInt32 MaterialID;
	        public byte Location;
	        public byte InNumber;
	        public byte OutNumber;
	        public byte ReturnNumber;	
        };
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        [Serializable()]
        public struct NarcoticStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	        public byte[] CardHead;
            public UInt16 Length;
	        public UInt16 Checkout;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	        public byte[] Version;
            public byte Reserve_1;

            public float BoxWeight;
	        public byte State;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserve_2;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	        public byte[] RoomNub;	
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	        public byte[] InName;
	        public UInt32  InTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	        public byte[] OutName;
	        public UInt32  OutTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	        public byte[] ReturnName;
	        public UInt32  ReturnTime;
	
	        public UInt32 DetailedNum;
            public byte DetailedMerge;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserve;
	        public UInt32 MaterialQty;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	        public NarcoticMaterialStruct[] Material;
        };
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        [Serializable()]
        public struct LegoStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] CardHead;
            public float Weight;
        };
        public enum WeightCommand
        {
            [Description("设置传感器使能")]
            WeightSetEnable = 0x01,
            [Description("查询传感器使能")]
            WeightQueryEnable = 0x02,
            [Description("设置量程")]
            WeightSetRange = 0x10,
            [Description("查询量程")]
            WeightQueryRange = 0x11,
            [Description("设置安全过载")]
            WeightSetSafeOverload = 0x12,
            [Description("查询安全过载")]
            WeightQuerySafeOverload = 0x13,
            [Description("设置最大过载")]
            WeightSetMaxOverload = 0x14,
            [Description("查询最大过载")]
            WeightQueryMaxOverload = 0x15,
            [Description("设置精度")]
            WeightSetPrecision = 0x16,
            [Description("查询精度")]
            WeightQueryPrecision = 0x17,
            [Description("设置传感器校准重量")]
            WeightSetSensorCalibrationWeight = 0x20,
            [Description("查询传感器校准重量")]
            WeightQuerySensorCalibrationWeight = 0x21,
            [Description("校准传感器")]
            WeightSensorCalibration = 0x22,
            [Description("设置传感器K值")]
            WeightSetSensorKValue = 0x23,
            [Description("查询K值")]
            WeightQuerySensorKValue = 0x24,
            [Description("查询AD ")]
            WeightQuerySensorADValue = 0x30,
            [Description("清零")]
            WeightSensorCleared = 0x31,
            [Description("查询零值")]
            WeightQueryZeroValue = 0x32,
            [Description("去皮")]
            WeightPeeling = 0x33,
            [Description("查询皮值")]
            WeightQueryPeeling = 0x34,
            [Description("查询传感器重量")]
            WeightQuerySensorWeight = 0x40,
            [Description("查询秤盘重量")]
            WeightQueryPanWeight = 0x41,
            [Description("设置纠偏门限")]
            WeightSetDriftCritical = 0x42,
            [Description("查询纠偏门限")]
            WeightQueryDriftCritical = 0x43,
            [Description("查询纠偏后重量")]
            WeightQueryDriftWeight = 0x44,
            [Description("设置上报门限")]
            WeightSetReportCritical = 0x47,
            [Description("查询上报门限")]
            WeightQueryReportCritical = 0x48,
            [Description("上报重量差")]
            WeightReportDiffWeight = 0x49,
            [Description("设置秤盘稳态参数")]
            WeightSetSteatyPara = 0x4A,
            [Description("查询秤盘稳态参数")]
            WeightGetSteatyPara = 0x4B,
            [Description("设置秤盘与传感器对应关系")]
            WeightSetPanAndSensor = 0x50,
            [Description("查询秤盘与传感器对应关系")]
            WeightQueryPanAndSensor = 0x51,
            [Description("设置传感器与秤盘对应关系")]
            WeightSetSensorAndPan = 0x52,
            [Description("查询传感器与秤盘对应关系")]
            WeightQuerySensorAndPan = 0x53,
            [Description("查询秤盘原始重量")]
            WeightQueryRawWeight = 0x60,
            [Description("上报秤盘原始重量")]
            WeightReportRawWeight = 0x61,
            [Description("设置秤盘原始上报周期")]
            WeightSetRawReportCycle = 0x62,
            [Description("查询秤盘原始上报周期")]
            WeightGetRawReportCycle = 0x63,
            [Description("设置秤盘原始上报门限")]
            WeightSetRawReportLevel = 0x64,
            [Description("查询秤盘原始上报门限")]
            WeightGetRawReportLevel = 0x65,
            [Description("查询秤盘开门重量")]
            WeightQueryOpenDoorWeight = 0x71,
        }
        public byte[] WeightSetEnable(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetEnable, Paramter);
            return sendData;
        }
        public byte[] WeightQueryEnable(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryEnable, new byte[1] { 0 });
            return sendData;
        }
        public byte[] WeightSetRange(ushort uDestinationAddress, UInt16 SensorIndex, float[] Range)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(SensorIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((SensorIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(Range[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetRange, Paramter);
            return sendData;
        }
        public byte[] WeightQueryRange(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryRange, Paramter);
            return sendData;
        }
        public byte[] WeightSetSafeOverload(ushort uDestinationAddress, UInt16 SensorIndex, float[] SafeOver)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(SensorIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((SensorIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(SafeOver[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len); 
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetSafeOverload, Paramter);
            return sendData;
        }
        public byte[] WeightQuerySafeOverload(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQuerySafeOverload, Paramter);
            return sendData;
        }
        public byte[] WeightSetMaxOverload(ushort uDestinationAddress, UInt16 SensorIndex, float[] MaxOver)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(SensorIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((SensorIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(MaxOver[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len); 
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetMaxOverload, Paramter);
            return sendData;
        }
        public byte[] WeightQueryMaxOverload(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryMaxOverload, Paramter);
            return sendData;
        }
        public byte[] WeightSetPrecision(ushort uDestinationAddress, UInt16 SensorIndex,float[] Precision)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(SensorIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((SensorIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(Precision[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);  
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetPrecision, Paramter);
            return sendData;
        }
        public byte[] WeightQueryPrecision(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryPrecision, Paramter);
            return sendData;
        }
        public byte[] WeightSetSensorCalibrationWeight(ushort uDestinationAddress, UInt16 SensorIndex, float[] CalWeight)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(SensorIndex));
            len += sizeof(UInt16);
            for (int i = 0,j=0; i < 16; ++i)
            {
                if ((SensorIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(CalWeight[j++]));
                    len += sizeof(float);                    
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len); 
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetSensorCalibrationWeight, Paramter);
            return sendData;
        }
        public byte[] WeightQuerySensorCalibrationWeight(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQuerySensorCalibrationWeight, Paramter);
            return sendData;
        }
        public byte[] WeightSensorCalibration(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSensorCalibration, Paramter);
            return sendData;
        }
        public byte[] WeightSetSensorKValue(ushort uDestinationAddress, UInt16 SensorIndex, float[] k)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(SensorIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((SensorIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(k[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);            
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetSensorKValue, Paramter);
            return sendData;
        }
        public byte[] WeightQuerySensorKValue(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQuerySensorKValue, Paramter);
            return sendData;
        }
        public byte[] WeightQuerySensorADValue(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQuerySensorADValue, Paramter);
            return sendData;
        }
        public byte[] WeightSensorCleared(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSensorCleared, Paramter);
            return sendData;
        }
        public byte[] WeightQueryZeroValue(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryZeroValue, Paramter);
            return sendData;
        }
        public byte[] WeightPeeling(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightPeeling, Paramter);
            return sendData;
        }
        public byte[] WeightQueryPeeling(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryPeeling, Paramter);
            return sendData;
        }
        public byte[] WeightQuerySensorWeight(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQuerySensorWeight, Paramter);
            return sendData;
        }
        public byte[] WeightQueryPanWeight(ushort uDestinationAddress, UInt16 PanIndex)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(PanIndex));
            len += sizeof(UInt16);
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryPanWeight, Paramter);
            return sendData;
        }
        public byte[] WeightSetDriftCritical(ushort uDestinationAddress,UInt16 PanIndex, float[] value)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(PanIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((PanIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(value[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetDriftCritical, Paramter);
            return sendData;
        }
        public byte[] WeightQueryDriftCritical(ushort uDestinationAddress,UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryDriftCritical, Paramter);
            return sendData;
        }
        public byte[] WeightQueryDriftWeight(ushort uDestinationAddress,UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryDriftWeight, Paramter);
            return sendData;
        }
         public byte[] WeightSetReportCritical(ushort uDestinationAddress,UInt16 PanIndex, float[] value)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(PanIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((PanIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(value[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetReportCritical, Paramter);
            return sendData;
        }
        public byte[] WeightQueryReportCritical(ushort uDestinationAddress,UInt16 PanIndex)
         {
             byte[] Paramter = BitConverter.GetBytes(PanIndex);
             byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryReportCritical, Paramter);
             return sendData;
         }
        public byte[] WeightSetSteatyPara(ushort uDestinationAddress, UInt16 DataNum, float Percent, float max)
        {
            byte[] Paramter = new byte[10];
            Array.Copy(BitConverter.GetBytes(DataNum), 0, Paramter, 0, 2);
            Array.Copy(BitConverter.GetBytes(Percent), 0, Paramter, 2, 4);
            Array.Copy(BitConverter.GetBytes(max), 0, Paramter, 6, 4);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetSteatyPara, Paramter);
            return sendData;
        }
        public byte[] WeightGetSteatyPara(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightGetSteatyPara, new byte[1] { 0 });
            return sendData;
        }
        public byte[] WeightSetPanAndSensor(ushort uDestinationAddress,UInt16 PanIndex, UInt16[] PanSensorIndex)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(PanIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((PanIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(PanSensorIndex[i]));
                    len += sizeof(UInt16);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetPanAndSensor, Paramter);
            return sendData;
        }
        public byte[] WeightQueryPanAndSensor(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryPanAndSensor, Paramter);
            return sendData;
        }
        public byte[] WeightSetSensorAndPan(ushort uDestinationAddress, UInt16 SensorIndex, byte[] PanNum)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(SensorIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((SensorIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.Add(PanNum[i]);
                    len += 1;
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetSensorAndPan, Paramter);
            return sendData;
        }
        public byte[] WeightQuerySensorAndPan(ushort uDestinationAddress, UInt16 SensorIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(SensorIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQuerySensorAndPan, Paramter);
            return sendData;
        }
        public byte[] WeightQueryRawWeight(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryRawWeight, Paramter);
            return sendData;
        }
        public byte[] WeightSetRawReportCycle(ushort uDestinationAddress, UInt16 PanIndex, UInt16[] Cycle)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(PanIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((PanIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(Cycle[i]));
                    len += sizeof(UInt16);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetRawReportCycle, Paramter);
            return sendData;
        }
        public byte[] WeightGetRawReportCycle(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightGetRawReportCycle, Paramter);
            return sendData;
        }

        public byte[] WeightSetRawReportLevel(ushort uDestinationAddress, UInt16 PanIndex, float[] Level)
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(PanIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((PanIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(Level[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightSetRawReportLevel, Paramter);
            return sendData;
        }
        public byte[] WeightGetRawReportLevel(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightGetRawReportLevel, Paramter);
            return sendData;
        }
        public byte[] WeightQueryOpenDoorWeight(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Weigh + (ushort)WeightCommand.WeightQueryOpenDoorWeight, Paramter);
            return sendData;
        }
        public enum DoorKockLampCommand
        {
            [Description("开锁")]
            LockOpen = 0x10,
            [Description("查询锁状态")]
            LockOpenGet = 0x20,
            [Description("查询门状态")]
            DoorOpenGet = 0x21,
            [Description("上报锁状态")]
            LockOpenReport = 0x30,
            [Description("上报门状态")]
            DoorOpenReport = 0x31,
        }
        public byte[] DllLockOpen(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.DoorKockLamp + (ushort)DoorKockLampCommand.LockOpen, new byte[1] { 0 });
            return sendData;
        }
        public byte[] DllLockOpenGet(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.DoorKockLamp + (ushort)DoorKockLampCommand.LockOpenGet, new byte[1] { 0 });
            return sendData;
        }
        public byte[] DllDoorOpenGet(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.DoorKockLamp + (ushort)DoorKockLampCommand.DoorOpenGet, new byte[1] { 0 });
            return sendData;
        }

        public enum LANCommand
        {            
            [Description("查询设备")]
            LANGetDevices = 0x01,
            [Description("读取参数")]
            LANGetParameter = 0x02,
            [Description("设置参数")]
            LANSetParameter = 0x03,
        }
        public byte[] LANGetDevices(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LAN + (ushort)LANCommand.LANGetDevices, new byte[1] { 0 });
            return sendData;
        }
        public byte[] LANGetParameter(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LAN + (ushort)LANCommand.LANGetParameter, new byte[1] { 0 });
            return sendData;
        }
        public byte[] LANSetParameter(ushort uDestinationAddress, byte[] mac, byte[] ip, byte[] mask, byte[] gateway, byte[] serverip, UInt16 serverport)
        {
            byte[] Paramter = new byte[24];
            Array.Copy(mac, 0, Paramter, 0, 6);
            Array.Copy(ip, 0, Paramter, 6, 4);
            Array.Copy(mask, 0, Paramter, 10, 4);
            Array.Copy(gateway, 0, Paramter, 14, 4);
            Array.Copy(serverip, 0, Paramter, 18, 4);
            Array.Copy(BitConverter.GetBytes(serverport), 0, Paramter, 22, 2);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LAN + (ushort)LANCommand.LANSetParameter, Paramter);
            return sendData;
        }
        public enum GoodsCommand
        {
            [Description("设置取用信息")]
            GoodsSetOut = 0x50,
            [Description("设置退还信息")]
            GoodsSetReturn = 0x51,
            [Description("设置状态")]
            GoodsSetState = 0x52,
            [Description("设置单品重量")]
            GoodsSetWeight = 0x60,
            [Description("查询单品重量")]
            GoodsGetWeight = 0x61,
            [Description("查询货物数量")]
            GoodsGetSteadyQuantity = 0x62,
            [Description("设置上报使能")]
            GoodsSetReportEnable = 0x63,
            [Description("查询上报使能")]
            GoodsGetReportEnable = 0x64,
            [Description("上报货物数量")]
            GoodsReportQuantity = 0x65,
            [Description("查询纠偏数量")]
            GoodsGetDriftQuantity = 0x66,
            [Description("查询原始数量")]
            GoodsGetRawQuantity = 0x71,
            [Description("查询开门计重数量")]
            GoodsGetOpenDoorQuantity = 0x81,
        }
        public byte[] GoodsSetOut(ushort uDestinationAddress,UInt32 time, byte[] name)
        {
            int len;
            if (name.Length > 8)
                len = 8;
            else
                len = name.Length;
            byte[] Paramter = new byte[4+len];
            Array.Copy(BitConverter.GetBytes(time), 0, Paramter, 0, 4);            
            Array.Copy(name, 0, Paramter, 4, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsSetOut, Paramter);
            return sendData;
        }
        public byte[] GoodsSetReturn(ushort uDestinationAddress, UInt32 time, byte[] name)
        {
            int len;
            if (name.Length > 8)
                len = 8;
            else
                len = name.Length;
            byte[] Paramter = new byte[4 + len];
            Array.Copy(BitConverter.GetBytes(time), 0, Paramter, 0, 4);
            Array.Copy(name, 0, Paramter, 4, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsSetReturn, Paramter);
            return sendData;
        }
        public byte[] GoodsSetState(ushort uDestinationAddress, byte state)
        {
            byte[] Paramter = new byte[1]{state};
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsSetState, Paramter);
            return sendData;
        }
        
        public byte[] GoodsSetWeight(ushort uDestinationAddress, UInt16 PanIndex, float[] weight )
        {
            List<byte> ParamterBuffer = new List<byte>(4096);
            int len = 0;
            ParamterBuffer.AddRange(BitConverter.GetBytes(PanIndex));
            len += sizeof(UInt16);
            for (int i = 0; i < 16; ++i)
            {
                if ((PanIndex & (1 << i)) != 0)
                {
                    ParamterBuffer.AddRange(BitConverter.GetBytes(weight[i]));
                    len += sizeof(float);
                }
            }
            byte[] Paramter = new byte[len];
            ParamterBuffer.CopyTo(0, Paramter, 0, len);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsSetWeight, Paramter);
            return sendData;
        }
        public byte[] GoodsGetWeight(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsGetWeight, Paramter);
            return sendData;
        }
        public byte[] GoodsSetReportEnable(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsSetReportEnable, Paramter);
            return sendData;
        }
        public byte[] GoodsGetReportEnable(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsGetReportEnable, new byte[1] { 0 });
            return sendData;
        }
        public byte[] GoodsGetSteadyQuantity(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsGetSteadyQuantity, Paramter);
            return sendData;
        }
        public byte[] GoodsGetDriftQuantity(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsGetDriftQuantity, Paramter);
            return sendData;
        }
        public byte[] GoodsGetRawQuantity(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsGetRawQuantity, Paramter);
            return sendData;
        }
        public byte[] GoodsGetOpenDoorQuantity(ushort uDestinationAddress, UInt16 PanIndex)
        {
            byte[] Paramter = BitConverter.GetBytes(PanIndex);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Goods + (ushort)GoodsCommand.GoodsGetOpenDoorQuantity, Paramter);
            return sendData;
        }
        public enum DisplayCommand
        {
            [Description("设置清屏")]
            DisplayClean = 0x01,
            [Description("显示字符")]
            DisplayShowChars = 0x02,
            [Description("清屏显字")]
            DisplayShowCleanChars = 0x03,
            [Description("特殊标记")]
            DisplayShowStamp = 0x04,
            [Description("清屏显示字符")]
            DisplayShowCleanAndChars = 0x05,
            [Description("清屏特殊标记")]
            DisplayShowCleanCharsStamp = 0x06,
            [Description("开引导")]
            DisplayGuideOpen = 0x20,
            [Description("关引导")]
            DisplayGuideClose = 0x21,
            [Description("开关屏幕")]
            DisplayOpen = 0x30,
            [Description("字符存储容量")]
            DisplayShowCleanCharsLineCapacity = 0x40,
            [Description("清屏显字每行格式")]
            DisplayShowCleanCharsLineForm = 0x41,
            [Description("清屏显字每行内容")]
            DisplayShowCleanCharsLineText = 0x42,
            [Description("清屏显字每行内容翻屏")]
            DisplayShowCleanCharsLineTextPage = 0x43,
            [Description("字符占用容量")]
            DisplayShowCleanCharsLineCapacityUsed = 0x44,
            [Description("设置自动关屏")]
            DisplayShowSetAutoOffScreen = 0x51,
            [Description("查询自动关屏")]
            DisplayShowGetAutoOffScreen = 0x52,
            [Description("设置数量显示样式")]
            DisplayShowSetWQStyle = 0x53,
            [Description("查询数量显示样式")]
            DisplayShowGetWQStyle = 0x54,
        }
        public byte[] DisplayOpen(ushort uDestinationAddress, byte state)
        {
            byte[] Paramter = new byte[1] { state };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayOpen, Paramter);
            return sendData;
        }
        public byte[] DisplayClean(ushort uDestinationAddress, UInt16 posX, UInt16 posY, UInt16 lenX, UInt16 lenY, Color color)
        {
            byte[] Paramter = new byte[12];
            Array.Copy(BitConverter.GetBytes(posX), 0, Paramter, 0, 2);
            Array.Copy(BitConverter.GetBytes(posY), 0, Paramter, 2, 2);
            Array.Copy(BitConverter.GetBytes(lenX), 0, Paramter, 4, 2);
            Array.Copy(BitConverter.GetBytes(lenY), 0, Paramter, 6, 2);
            Array.Copy(BitConverter.GetBytes(color.ToArgb()), 0, Paramter, 8, 4);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayClean, Paramter);
            return sendData;
        }
        public byte[] DisplayShowChars(ushort uDestinationAddress, UInt16 posX, UInt16 posY, float size, Color colorF, Color colorB, byte[] chars)
        {
            byte[] Paramter = new byte[chars.Length + 16];
            Array.Copy(BitConverter.GetBytes(posX), 0, Paramter, 0, 2);
            Array.Copy(BitConverter.GetBytes(posY), 0, Paramter, 2, 2);
            Array.Copy(BitConverter.GetBytes(size), 0, Paramter, 4, 4);
            Array.Copy(BitConverter.GetBytes(colorF.ToArgb()), 0, Paramter, 8, 4); 
            Array.Copy(BitConverter.GetBytes(colorB.ToArgb()), 0, Paramter, 12, 4);
            Array.Copy(chars, 0, Paramter, 16, chars.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowChars, Paramter);
            return sendData;
        }
        public byte[] DisplayShowCleanChars(ushort uDestinationAddress, UInt16 posX, UInt16 posY, float size, Color colorF, Color colorB, byte[] chars)
        {
            byte[] Paramter = new byte[chars.Length + 16];
            Array.Copy(BitConverter.GetBytes(posX), 0, Paramter, 0, 2);
            Array.Copy(BitConverter.GetBytes(posY), 0, Paramter, 2, 2);
            Array.Copy(BitConverter.GetBytes(size), 0, Paramter, 4, 4);
            Array.Copy(BitConverter.GetBytes(colorF.ToArgb()), 0, Paramter, 8, 4);
            Array.Copy(BitConverter.GetBytes(colorB.ToArgb()), 0, Paramter, 12, 4);
            Array.Copy(chars, 0, Paramter, 16, chars.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowCleanChars, Paramter);
            return sendData;
        }
        public byte[] DisplayShowStamp(ushort uDestinationAddress, UInt16 posX, UInt16 posY, float size, Color colorF, Color colorB, byte[] chars)
        {
            byte[] Paramter = new byte[chars.Length + 16];
            Array.Copy(BitConverter.GetBytes(posX), 0, Paramter, 0, 2);
            Array.Copy(BitConverter.GetBytes(posY), 0, Paramter, 2, 2);
            Array.Copy(BitConverter.GetBytes(size), 0, Paramter, 4, 4);
            Array.Copy(BitConverter.GetBytes(colorF.ToArgb()), 0, Paramter, 8, 4);
            Array.Copy(BitConverter.GetBytes(colorB.ToArgb()), 0, Paramter, 12, 4);
            Array.Copy(chars, 0, Paramter, 16, chars.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowStamp, Paramter);
            return sendData;
        }
        public byte[] DisplayShowCleanAndChars(ushort uDestinationAddress, UInt16 CposX, UInt16 CposY, UInt16 lenX, UInt16 lenY, UInt16 SposX, UInt16 SposY, float size, Color colorF, Color colorB, byte[] chars)
        {
            byte[] Paramter = new byte[chars.Length + 24];
            Array.Copy(BitConverter.GetBytes(CposX), 0, Paramter, 0, 2);
            Array.Copy(BitConverter.GetBytes(CposY), 0, Paramter, 2, 2);
            Array.Copy(BitConverter.GetBytes(lenX), 0, Paramter, 4, 2);
            Array.Copy(BitConverter.GetBytes(lenY), 0, Paramter, 6, 2);
            Array.Copy(BitConverter.GetBytes(SposX), 0, Paramter, 8, 2);
            Array.Copy(BitConverter.GetBytes(SposY), 0, Paramter, 10, 2);
            Array.Copy(BitConverter.GetBytes(size), 0, Paramter, 12, 4);
            Array.Copy(BitConverter.GetBytes(colorF.ToArgb()), 0, Paramter, 16, 4);
            Array.Copy(BitConverter.GetBytes(colorB.ToArgb()), 0, Paramter, 20, 4);
            Array.Copy(chars, 0, Paramter, 24, chars.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowCleanAndChars, Paramter);
            return sendData;
        }
        public byte[] DisplayShowCleanCharsStamp(ushort uDestinationAddress, UInt16 CposX, UInt16 CposY, UInt16 lenX, UInt16 lenY, UInt16 SposX, UInt16 SposY, float size, Color colorF, Color colorB, byte[] chars)
        {
            byte[] Paramter = new byte[chars.Length + 24];
            Array.Copy(BitConverter.GetBytes(CposX), 0, Paramter, 0, 2);
            Array.Copy(BitConverter.GetBytes(CposY), 0, Paramter, 2, 2);
            Array.Copy(BitConverter.GetBytes(lenX), 0, Paramter, 4, 2);
            Array.Copy(BitConverter.GetBytes(lenY), 0, Paramter, 6, 2);
            Array.Copy(BitConverter.GetBytes(SposX), 0, Paramter, 8, 2);
            Array.Copy(BitConverter.GetBytes(SposY), 0, Paramter, 10, 2);
            Array.Copy(BitConverter.GetBytes(size), 0, Paramter, 12, 4);
            Array.Copy(BitConverter.GetBytes(colorF.ToArgb()), 0, Paramter, 16, 4);
            Array.Copy(BitConverter.GetBytes(colorB.ToArgb()), 0, Paramter, 20, 4);
            Array.Copy(chars, 0, Paramter, 24, chars.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowCleanCharsStamp, Paramter);
            return sendData;
        }
        public byte[] DisplayGuideOpen(ushort uDestinationAddress, Color color, UInt16 delay, byte flash)
        {
            byte[] Paramter = new byte[7];                           
            Array.Copy(BitConverter.GetBytes(color.ToArgb()), 0, Paramter, 0, 4);
            Array.Copy(BitConverter.GetBytes(delay), 0, Paramter, 4, 2);
            Array.Copy(BitConverter.GetBytes(flash), 0, Paramter, 6, 1);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayGuideOpen, Paramter);
            return sendData;
        }
        public byte[] DisplayGuideClose(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayGuideClose, new byte[1] { 0 });
            return sendData;
        }
        public byte[] DisplayShowCleanCharsLineCapacity(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowCleanCharsLineCapacity, new byte[1] { 0 });
            return sendData;
        }
        public byte[] DisplayShowCleanCharsLineCapacityUsed(ushort uDestinationAddress)
        {
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowCleanCharsLineCapacityUsed, new byte[1] { 0 });
            return sendData;
        }
        public byte[] DisplayShowCleanCharsLineForm(ushort uDestinationAddress, Color colorB, Color color1, float size1, byte align1,
                                                                                            Color color2, float size2, byte align2,
                                                                                            Color color3, float size3, byte align3,
                                                                                            Color color4, float size4, byte align4,
                                                                                            Color color5, float size5, byte align5,
                                                                                            Color color6, float size6, byte align6)
        {
            byte[] Paramter = new byte[58];
            Array.Copy(BitConverter.GetBytes(colorB.ToArgb()), 0, Paramter, 0, 4);
            Array.Copy(BitConverter.GetBytes(color1.ToArgb()), 0, Paramter, 4, 4);
            Array.Copy(BitConverter.GetBytes(size1), 0, Paramter, 8, 4);
            Array.Copy(BitConverter.GetBytes(align1), 0, Paramter, 12, 1);
            Array.Copy(BitConverter.GetBytes(color2.ToArgb()), 0, Paramter, 13, 4);
            Array.Copy(BitConverter.GetBytes(size2), 0, Paramter, 17, 4);
            Array.Copy(BitConverter.GetBytes(align2), 0, Paramter, 21, 1);
            Array.Copy(BitConverter.GetBytes(color3.ToArgb()), 0, Paramter, 22, 4);
            Array.Copy(BitConverter.GetBytes(size3), 0, Paramter, 26, 4);
            Array.Copy(BitConverter.GetBytes(align3), 0, Paramter, 30, 1);
            Array.Copy(BitConverter.GetBytes(color4.ToArgb()), 0, Paramter, 31, 4);
            Array.Copy(BitConverter.GetBytes(size4), 0, Paramter, 35, 4);
            Array.Copy(BitConverter.GetBytes(align4), 0, Paramter, 39, 1);
            Array.Copy(BitConverter.GetBytes(color5.ToArgb()), 0, Paramter, 40, 4);
            Array.Copy(BitConverter.GetBytes(size5), 0, Paramter, 44, 4);
            Array.Copy(BitConverter.GetBytes(align5), 0, Paramter, 48, 1);
            Array.Copy(BitConverter.GetBytes(color6.ToArgb()), 0, Paramter, 49, 4);
            Array.Copy(BitConverter.GetBytes(size6), 0, Paramter, 53, 4);
            Array.Copy(BitConverter.GetBytes(align6), 0, Paramter, 57, 1);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowCleanCharsLineForm, Paramter);
            return sendData;

        }
        public byte[] DisplayShowCleanCharsLineText(ushort uDestinationAddress, byte save, byte[] chars)
        {
            byte[] Paramter = new byte[chars.Length + 1];
            Array.Copy(BitConverter.GetBytes(save), 0, Paramter, 0, 1);
            Array.Copy(chars, 0, Paramter, 1, chars.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowCleanCharsLineText, Paramter);
            return sendData;
        }
        public byte[] DisplayShowCleanCharsLineTextPage(ushort uDestinationAddress, byte save, byte add, byte[] chars)
        {
            byte[] Paramter = new byte[chars.Length + 2];
            Array.Copy(BitConverter.GetBytes(save), 0, Paramter, 0, 1);
            Array.Copy(BitConverter.GetBytes(add), 0, Paramter, 1, 1);
            Array.Copy(chars, 0, Paramter, 2, chars.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowCleanCharsLineTextPage, Paramter);
            return sendData;
        }
        public byte[] DisplayShowSetAutoOffScreen(ushort uDestinationAddress, byte state)
        {
            byte[] Paramter = new byte[1] { state };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowSetAutoOffScreen, Paramter);
            return sendData;
        }
        public byte[] DisplayShowGetAutoOffScreen(ushort uDestinationAddress, byte state)
        {
            byte[] Paramter = new byte[1] { state };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowGetAutoOffScreen, Paramter);
            return sendData;
        }
        public byte[] DisplayShowSetWQStyle(ushort uDestinationAddress, byte IsWeight, byte IsQuntity, byte state)
        {
            byte[] Paramter = new byte[3] { IsWeight, IsQuntity,state };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowSetWQStyle, Paramter);
            return sendData;
        }
        public byte[] DisplayShowGetWQStyle(ushort uDestinationAddress, byte state)
        {
            byte[] Paramter = new byte[1] { state };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.Display + (ushort)DisplayCommand.DisplayShowGetWQStyle, Paramter);
            return sendData;
        }

        public enum LEDCommand
        {
            [Description("查询LED样式")]
            LEDGetStyle = 0x01,
            [Description("设置LED样式")]
            LEDSetStyle = 0x02,
            [Description("设置LED颜色")]
            LEDSetColour = 0x10,
            [Description("设置组LED颜色")]
            LEDSetColourGroup = 0x11,
            [Description("查询LED亮度")]
            LEDGetBright = 0x21,
            [Description("设置LED亮度")]
            LEDSetBright = 0x22,
        }
        public byte[] LEDGetStyle(ushort uDestinationAddress)
        {
            byte[] Paramter = new byte[1] { 0 };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LED + (ushort)LEDCommand.LEDGetStyle, Paramter);
            return sendData;
        }
        public byte[] LEDSetStyle(ushort uDestinationAddress, byte style)
        {
            byte[] Paramter = new byte[1] { style };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LED + (ushort)LEDCommand.LEDSetStyle, Paramter);
            return sendData;
        }
        public byte[] LEDSetColour(ushort uDestinationAddress, UInt32 index, byte[] colour)
        {
            byte[] Paramter = new byte[colour.Length + 4];
            Array.Copy(BitConverter.GetBytes(index), 0, Paramter, 0, 4);
            Array.Copy(colour, 0, Paramter, 4, colour.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LED + (ushort)LEDCommand.LEDSetColour, Paramter);
            return sendData;
        }
        public byte[] LEDSetColourGroup(ushort uDestinationAddress,UInt16 index, byte[] colour)
        {
            byte[] Paramter = new byte[colour.Length + 2];
            Array.Copy(BitConverter.GetBytes(index), 0, Paramter, 0, 2);
            Array.Copy(colour, 0, Paramter, 2, colour.Length);
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LED + (ushort)LEDCommand.LEDSetColourGroup, Paramter);
            return sendData;
        }
        public byte[] LEDGetBright(ushort uDestinationAddress)
        {
            byte[] Paramter = new byte[1] { 0 };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LED + (ushort)LEDCommand.LEDGetBright, Paramter);
            return sendData;
        }
        public byte[] LEDSetBright(ushort uDestinationAddress, byte style)
        {
            byte[] Paramter = new byte[1] { style };
            byte[] sendData = Serializer.Instance.Serialize(uDestinationAddress, (ushort)CommandTypes.LED + (ushort)LEDCommand.LEDSetBright, Paramter);
            return sendData;
        }

    }

}
