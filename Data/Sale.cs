namespace WpfAppCon02
{
    public class Sale
    {
        public int SNR { get; set; }

        public int CID { get; set; }

        public int OID { get; set; }

        public Sale(int snr, int cid, int oid)
        {
            SNR = snr;
            CID = cid;
            OID = oid;
        }

        public Sale(int cid, int oid)
        {
            CID = cid;
            OID = oid;
        }

        public override string ToString()
        {
            return "{ snr=" + SNR + ", cid=" + CID + ", oid=" + OID + "}";
        }
    }
}