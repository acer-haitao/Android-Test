using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class DisplayUtils
    {
        private static DisplayUtils instance = new DisplayUtils();
        public static DisplayUtils Instance
        {
            get { return instance; }
        }

        public DisplayUtils() { ; }
    }
}
