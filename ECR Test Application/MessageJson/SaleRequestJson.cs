using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wcfServer.Services
{
    public class SaleRequestJson
    {
        public string transType { get; set; }
        public float amount { get; set; }
        public long tip { get; set; }
        public string remark { get; set; }
        
    }
}
