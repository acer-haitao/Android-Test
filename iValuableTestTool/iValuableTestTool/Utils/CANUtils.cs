using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class CANUtils
    {
        private static CANUtils instance = new CANUtils();
        public static CANUtils Instance
        {
            get { return instance; }
        }

        public CANUtils() { ; }
    }
}
