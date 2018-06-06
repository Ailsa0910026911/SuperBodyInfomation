using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class APPModule
    {
        private string cols = "Name,PictureUrl,PicUrl,Value,ModuleType,Sort";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}