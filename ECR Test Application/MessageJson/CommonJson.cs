using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wcfServer.Services
{
    public class CommonJson
    {
        public string resultcode { get; set; }
        public string status { get; set; }

        public string invoiceNo { get; set; }
        public string message { get; set; }
        
        public string transactionAmount { get; set; }
        public string transactionDate { get; set; }
        public string transactionTime { get; set; }
        public string transactionType { get; set; }
        public string verifyTransId { get; set; }


    }
}
