using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class OrdersModel
    {
        private string cols = "Id,Name";
        private int id;
        private string name;

        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
