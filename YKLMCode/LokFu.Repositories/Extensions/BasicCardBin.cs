using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class BasicCardBin
    {
        private string cols = "Id";
        private string card;

        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }

        public string Card
        {
            get { return card; }
            set { card = value; }
        }
    }
}
