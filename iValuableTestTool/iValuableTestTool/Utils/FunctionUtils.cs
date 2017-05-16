using DIH.CommunicationProtocol.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class FunctionUtils
    {
        private static FunctionUtils instance = new FunctionUtils();
        public static FunctionUtils Instance
        {
            get { return instance; }
        }
        public FunctionUtils() { ; }
        #region command
        [Description("分方命令")]
        public const ushort AssignJobs = (ushort)CommandTypes.Functions + (ushort)FunctionCommand.AssignJobs;
        [Description("呼叫复核命令")]
        public const ushort CallingCheck = (ushort)CommandTypes.Functions + (ushort)FunctionCommand.CallingCheck;
        [Description("复核完成命令")]
        public const ushort CheckFinished = (ushort)CommandTypes.Functions + (ushort)FunctionCommand.CheckFinished;
        [Description("呼叫绑定命令")]
        public const ushort CallingBinding = (ushort)CommandTypes.Functions + (ushort)FunctionCommand.CallingBinding;
        [Description("绑定完成命令")]
        public const ushort BindingFinished = (ushort)CommandTypes.Functions + (ushort)FunctionCommand.BindingFinished;
         [Description("发送绑定台忙的指令")]
        public const ushort BindingTableBusy = (ushort)CommandTypes.Functions + (ushort)FunctionCommand.BindingTableBusy;
        
        #endregion
        #region Event
   
        public byte[] SendCommand(byte[] presNo,ushort commandType)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(presNo);
            //bytes.Add(0);
            return CommunicationTools.Instance.GetSendData(commandType, bytes.ToArray());
        }
#endregion
    }
}
