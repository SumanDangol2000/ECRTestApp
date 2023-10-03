using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wcfServer.Services
{
    public class VoidSaleResponseJson
    {
        public string transType { get; set; }
        public string resultCode { get; set; }
        public double message { get; set; }
        public int mid { get; set; }
        public int tid { get; set; }
        public string merchantName { get; set; }
        public string amount { get; set; }
        public double cardNo { get; set; }
        public int date { get; set; }
        public int time { get; set; }
        public string entryMode { get; set; }
        public string traceNo { get; set; }
        public string bachNo { get; set; }
        public double referenceNo { get; set; }
        public string origTraceNo { get; set; }
        public string origOutOrderNo { get; set; }
        public string origReferenceNo { get; set; }
        public int organization { get; set; }
        public string currencyCode{get; set; }
        public string invoiceNo { get; set; }

    }
}
