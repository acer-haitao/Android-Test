using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class BluetoothUtils
    {
        private static BluetoothUtils instance = new BluetoothUtils();
        public static BluetoothUtils Instance
        {
            get { return instance; }
        }

        public BluetoothUtils() { ; }
    }
}
