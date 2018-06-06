using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class YYDaily
    {
        private string cols = "OutDate,ClickPV,ClickUV,ShakePV,ShakeUV";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}