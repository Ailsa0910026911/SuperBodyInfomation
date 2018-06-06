using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class JobSet
    {
        private string cols = "Cost,VIPCost,Cash,VIPCash,EqDays,MaxDay,MaxPay,MinMoney,MaxMoney,Floated,AdvCost,AdvCash,MaxRand,MinFloated,AdvFloated,MaxFloated,DayMoney,SetZhiNeng";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}