using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class JobItem
    {
        private string cols = "RunNum,RunMoney,RunTime,Poundage,State,RunType";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}