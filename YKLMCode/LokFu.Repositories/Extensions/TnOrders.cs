using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories.Extensions
{
    /// <summary>
    /// Tn统计
    /// </summary>
    public class TnOrders
    {
        public DateTime PayTime { get; set; }
        public int Counts { get; set; }
        public decimal All_Amoney { get; set; }
        public double All_AidpAyget { get; set; }
        public decimal All_AgentpAyget { get; set; }
    }
}
