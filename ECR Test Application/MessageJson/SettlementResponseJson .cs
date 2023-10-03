using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wcfServer.Services
{
    public class SettlementResponseJson
    {
        public string transType { get; set; }
        public string resultCode { get; set; }
        public string message { get; set; }
        public string TransactionDetail { get; set; }
        public string tid { get; set; }
        public string mid { get; set; }
        public string dateAndTime { get; set; }
        public string merchantName { get; set; }
        public int batchNo { get; set; }

    }
}
