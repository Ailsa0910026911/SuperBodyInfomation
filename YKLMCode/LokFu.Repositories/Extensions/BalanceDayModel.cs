using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories.Extensions
{
    public class BalanceDayModel
    {
        public DateTime SDATE { get; set; }
        public decimal INMONEY { get; set; }
        public decimal OUTMONEY { get; set; }
        public decimal AFALLMONEY { get; set; }
        public decimal INTEREST { get; set; }
    }
}
