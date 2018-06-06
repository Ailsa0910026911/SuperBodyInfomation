using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class BasicCarBrand
    {
        private string cols = "Id,Name,Score,Letter,Logo";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}