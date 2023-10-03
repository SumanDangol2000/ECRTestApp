using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wcfServer.Services
{
    public class SaleResponseJson
    {
        public string transType { get; set; }
        public string resultCode { get; set; }
        public string message { get; set; }
        public string mid { get; set; }
        public string tid { get; set; }
        public string merchantName { get; set; }
        public long amount { get; set; }
        public string cardNo { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public int entryMode { get; set; }
        public string traceNo { get; set; }
        public string batchNo { get; set; }
        public string referenceNo { get; set; }
        public string authCode { get; set; }
        public string organization { get; set; }
        public string currencyCode{get; set; }
        public string invoiceNo { get; set; }

    }
}
