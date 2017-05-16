using DIH.CommunicationProtocol.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils
{
    public class RFIDUtils
    {
        private static RFIDUtils instance = new RFIDUtils();
        public static RFIDUtils Instance
        {
            get { return instance; }
        }

        public RFIDUtils() { ; }
        [Description("请求读卡")]
        public const ushort ResponseReadRFIDCard = (ushort)CommandTypes.RFID + (ushort)RFIDCommand.ResponseReadRFIDCard;
        [Description("设置RFID 内容")]
        public const ushort SetRFIDContent = (ushort)CommandTypes.RFID + (ushort)RFIDCommand.SetRFIDContent;
        [Description("卡片移除")]
        public const ushort ResponseReMoveRFIDCard = (ushort)CommandTypes.RFID + (ushort)RFIDCommand.ResponseReMoveRFIDCard;
        public const ushort QueryRFIDContent = (ushort)CommandTypes.RFID + (ushort)RFIDCommand.QueryRFIDContent;
        public RFIDModel GetModel(string presNo, string patientName)
        {
            RFIDModel rfid = new RFIDModel();
            rfid.CardHead = System.Text.Encoding.ASCII.GetBytes("DIHM");
            rfid.BlockQty = 12;
            rfid.Checkout = new byte[2] { 0, 0 };
            rfid.Version = new byte[3] { 2, 0, 0 };
            rfid.Reserve1 = new byte[2] { 0, 0 };
            rfid.Reserve2 = new byte[4] { 0, 0, 0, 0 };
            long time_t1 = (Convert.ToDateTime(DateTime.Now).ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            rfid.BindedTime = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes(time_t1), 0, rfid.BindedTime, 0, rfid.BindedTime.Length);
            rfid.PrescriptionNo = new byte[20];
            byte[] prescriptionNo = System.Text.Encoding.ASCII.GetBytes(presNo);
            prescriptionNo.CopyTo(rfid.PrescriptionNo, 0);
            char[] patientInfo = patientName.ToCharArray(0, patientName.Length);
            List<byte> bytes = new List<byte>();
            foreach (char name in patientInfo)
            {
                bytes.AddRange(BitConverter.GetBytes(name));
            }
            rfid.PatientName = bytes.ToArray();
            return rfid;
        }
        public byte[] GetSendSetContentBytes(string presNo, string patientName)
        {
            List<Byte> bytes = new List<byte>();
            RFIDModel model=GetModel(presNo,patientName);
            bytes.AddRange(model.CardHead);
            bytes.Add(model.BlockQty); 
            bytes.AddRange(model.Version);
            bytes.AddRange(model.Checkout);
            bytes.AddRange(model.Reserve1);
            bytes.AddRange(model.Reserve2);
            bytes.AddRange(model.BindedTime);
            bytes.AddRange(model.PrescriptionNo);
            bytes.AddRange(model.PatientName);
            return bytes.ToArray();
        }

        public byte[] SendQueryRFIDContent()
        {
            byte[] bytes = new byte[2] { 5, 5 };
            return CommunicationTools.Instance.GetSendData(QueryRFIDContent, bytes);
        }
        
    }
}
