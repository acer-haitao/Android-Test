using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class DoorLockLampUtils
    {
        private static DoorLockLampUtils instance = new DoorLockLampUtils();
        public static DoorLockLampUtils Instance
        {
            get { return instance; }
        }

        public DoorLockLampUtils() { ; }
    }
}
