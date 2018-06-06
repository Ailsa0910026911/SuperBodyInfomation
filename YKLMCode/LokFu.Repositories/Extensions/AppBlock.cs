using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class AppBlock
    {
        private string cols = "Name,IconUrl,LinkName1,LinkName2,LinkType,LinkType1,LinkType2,LinkUrl,LinkUrl1,LinkUrl2,PicUrl,Sort,SubName,Width,Height";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}