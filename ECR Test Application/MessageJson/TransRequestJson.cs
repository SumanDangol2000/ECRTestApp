using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wcfServer.Services
{
    public class TransRequestJson
    {
        public string uniqueTransId { get; set; }
        public string transType { get; set; }
        public float amount { get; set; }
        public long tip { get; set; }
        public string remark { get; set; }
        public string pat { get; set; }


    }
}
