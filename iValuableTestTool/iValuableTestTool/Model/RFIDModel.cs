using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Model
{
    public class RFIDModel
    {
        public byte[] CardHead { get; set; }
        public byte BlockQty { get; set; }
        public byte[] Version { get; set; }
        public byte[] Checkout { get; set; }
        public byte[] Reserve1 { get; set; }
        public byte[] Reserve2 { get; set; }
        public byte[] BindedTime { get; set; }
        public byte[] PrescriptionNo { get; set; }
        public byte[] PatientName { get; set; }
    }
}
