using System;

namespace WpfAppCon02
{
    public class Sale
    {
        public int SNR { get; set; }

        public int CID { get; set; }

        public int OID { get; set; }

        public DateTime SALEDATE { get; set; }

        public Sale(int snr, int cid, int oid, DateTime dateTime)
        {
            SNR = snr;
            CID = cid;
            OID = oid;
            SALEDATE = dateTime;
        }

        public Sale(int cid, int oid, DateTime dateTime)
        {
            CID = cid;
            OID = oid;
            SALEDATE = dateTime;
        }

        public override string ToString()
        {
            return "{ snr=" + SNR + ", cid=" + CID + ", oid=" + OID + "}";
        }
    }
}