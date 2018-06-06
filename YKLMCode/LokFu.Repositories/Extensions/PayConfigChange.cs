using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class PayConfigChange
    {
        private string cols = "Id,Title,APrice,BPrice,State,ShareNumber,ShowTip,SubTitle,ShareNumber";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}
