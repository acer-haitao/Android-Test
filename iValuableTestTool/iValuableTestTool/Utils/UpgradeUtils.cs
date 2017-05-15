using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class UpgradeUtils
    {
        private static UpgradeUtils instance = new UpgradeUtils();
        public static UpgradeUtils Instance
        {
            get { return instance; }
        }

        public UpgradeUtils() { ; }
    }
}
