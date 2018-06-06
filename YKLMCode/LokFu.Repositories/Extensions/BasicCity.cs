using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class BasicCity
    {
        private string cols = "Id,Name";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}