using DIH.CommunicationProtocol.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class WeightUtils
    {
        private static WeightUtils instance = new WeightUtils();
        public static WeightUtils Instance
        {
            get { return instance; }
        }

        public WeightUtils() { ; }
        #region command
        [Description("设置传感器使能")]
        public const ushort SetEnable = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SetEnable;
        [Description("查询传感器使能")]
        public const ushort QueryEnable = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QueryEnable;
        [Description("设置量程")]
        public const ushort SetRange = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SetRange;
        [Description("查询量程")]
        public const ushort QueryRange = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QueryRange;
        [Description("设置安全过载")]
        public const ushort SetSafeOverload = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SetSafeOverload;
        [Description("查询安全过载")]
        public const ushort QuerySafeOverload = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QuerySafeOverload;
        [Description("设置最大过载")]
        public const ushort SetMaxOverload = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SetMaxOverload;
        [Description("查询最大过载")]
        public const ushort QueryMaxOverload = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QueryMaxOverload;
        [Description("设置精度")]
        public const ushort SetPrecision = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SetPrecision;
        [Description("查询精度")]
        public const ushort QueryPrecision = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QueryPrecision;
        [Description("设置传感器校准重量")]
        public const ushort SetSensorCalibrationWeight = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SetSensorCalibrationWeight;
        [Description("查询传感器校准重量")]
        public const ushort QuerySensorCalibrationWeight = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QuerySensorCalibrationWeight;
        [Description("校准传感器")]
        public const ushort SensorCalibration = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SensorCalibration;
        [Description("设置传感器K值")]
        public const ushort SetSensorKValue = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SetSensorKValue;
        [Description("查询K值")]
        public const ushort QuerySensorKValue = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QuerySensorKValue;
        [Description("查询AD ")]
        public const ushort QuerySensorADValue = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QuerySensorADValue;
        [Description("清零")]
        public const ushort SensorCleared = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SensorCleared;
        [Description("查询零值")]
        public const ushort QueryZeroValue = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QueryZeroValue;
        [Description("去皮")]
        public const ushort Peeling = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.Peeling;
        [Description("查询皮值")]
        public const ushort QueryPeeling = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QueryPeeling;
        [Description("查询传感器重量")]
        public const ushort QuerySensorWeight = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QuerySensorWeight;
        [Description("查询秤盘重量")]
        public const ushort QueryPanWeight = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QueryPanWeight;
        [Description("设置秤盘与传感器对应关系")]
        public const ushort SetPanAndSensor = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.SetPanAndSensor;
        [Description("查询秤盘与传感器对应关系")]
        public const ushort QueryPanAndSensor = (ushort)CommandTypes.Weigh + (ushort)WeighCommand.QueryPanAndSensor;
        #endregion
        #region  receiveData

        public int[] ReceiveQueryEnable(InstructionStructure instruction)
        {
            return CommunicationTools.Instance.GetSelectedSensorIndex(instruction.CommandParameter);
        }
        
        public sbyte ReceiveResult(InstructionStructure instruction)
        {
            return CommunicationTools.Instance.GetReceiveResut(instruction);
        }
        #endregion
        #region SendData
        public byte[] SendQueryEnable()
        {
            byte[] bytes = new byte[1] { 0 };//固定0
            return CommunicationTools.Instance.GetSendData(QueryEnable, bytes);
        }
        public byte[] SendSetEnable(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(SetEnable, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendQueryRange(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QueryRange, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendSetRange(int[] selectedSensorIndex, float selectedRange)
        {
            return CommunicationTools.Instance.GetSendData(SetRange, CommunicationTools.Instance.SendSetValue(selectedSensorIndex, selectedRange));
        }
        public byte[] SendSetSensorCalibrationWeight(int[] selectedSensorIndex, float selectedValue)
        {
            return CommunicationTools.Instance.GetSendData(SetSensorCalibrationWeight,  CommunicationTools.Instance.SendSetValue(selectedSensorIndex, selectedValue));
        }

        public byte[] SendKValue(int[] selectedSensorIndex, float[] selectedValue)
        {
            
            return CommunicationTools.Instance.GetSendData(SetSensorKValue,  CommunicationTools.Instance.SendSetFloatValue(selectedSensorIndex, selectedValue));
        }
        public byte[] SendSensorCalibration(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(SensorCalibration, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendQuerySensorWeight(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QuerySensorWeight, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendQuerySensorKValue(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QuerySensorKValue, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendQuerySensorADValue(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QuerySensorADValue, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendSensorCleared(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(SensorCleared, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendQueryZeroValue(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QueryZeroValue, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendPeeling(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(Peeling, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendQueryPeeling(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QueryPeeling, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendQueryPanWeight(int[] selectedSensorIndex, float selectedValue)
        {
            return CommunicationTools.Instance.GetSendData(QueryPanWeight, CommunicationTools.Instance.SendSetValue(selectedSensorIndex, selectedValue));
            
        }
        public byte[] SendSetSafeOverload(int[] selectedSensorIndex, float selectedValue)
        {
            return CommunicationTools.Instance.GetSendData(SetSafeOverload, CommunicationTools.Instance.SendSetValue(selectedSensorIndex, selectedValue));
        }
        public byte[] SendQuerySafeOverload(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QuerySafeOverload, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendSetMaxOverload(int[] selectedSensorIndex, float selectedValue)
        {
            return CommunicationTools.Instance.GetSendData(SetMaxOverload, CommunicationTools.Instance.SendSetValue(selectedSensorIndex, selectedValue));
        }
        public byte[] SendQueryMaxOverload(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QueryMaxOverload, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendQueryPanAndSensor(int[] panIndex)
        {
            return CommunicationTools.Instance.GetSendData(QueryPanAndSensor, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(panIndex)));
        }
        public byte[] SendSetPanAndSensor(int[] panIndex, int[] selectedSensorIndex)
        {
             List<byte> bytes = new List<byte>();
             bytes.AddRange(BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(panIndex)));
            bytes.AddRange(BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
            return CommunicationTools.Instance.GetSendData(SetPanAndSensor, bytes.ToArray());
        }
        public byte[] SendQuerySensorCalibrationWeight(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QuerySensorCalibrationWeight, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }
        public byte[] SendSetPrecision(int[] selectedSensorIndex, float selectedRange)
        {
            return CommunicationTools.Instance.GetSendData(SetPrecision,CommunicationTools.Instance.SendSetValue(selectedSensorIndex, selectedRange));
        }
        public byte[] SendQueryPrecision(int[] selectedSensorIndex)
        {
            return CommunicationTools.Instance.GetSendData(QueryPrecision, BitConverter.GetBytes(CommunicationTools.Instance.GetSelectedSensorUshort(selectedSensorIndex)));
        }

        #endregion

    }
}
