using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class GoodsAttributeUtils
    {
        private static GoodsAttributeUtils instance = new GoodsAttributeUtils();
        public static GoodsAttributeUtils Instance
        {
            get { return instance; }
        }

        public GoodsAttributeUtils() { ; }
    }
}
