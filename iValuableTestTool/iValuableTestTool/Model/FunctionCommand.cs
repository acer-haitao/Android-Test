using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Model
{
    public enum FunctionCommand
    {
        [Description("发送分方命令")]
        AssignJobs = 0x01,
        [Description("发送呼叫复核命令")]
        CallingCheck = 0x02,
        [Description("发送复核完成命令")]
        CheckFinished = 0x03,
        [Description("发送呼叫绑定命令")]
        CallingBinding = 0x04,
        [Description("发送绑定完成命令")]
        BindingFinished = 0x05,
        [Description("发送绑定台忙的指令")]
        BindingTableBusy= 0x06,
    }
}
