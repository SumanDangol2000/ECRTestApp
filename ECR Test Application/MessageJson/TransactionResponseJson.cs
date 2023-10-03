using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wcfServer.Services
{
    public class TransactionResponseJson
    {
        public int ResponseType { get; set; }
        public string BillNo { get; set; }
        public double Amount { get; set; }
        public int TranType { get; set; }
        public int TranStatus { get; set; }

    }
}
