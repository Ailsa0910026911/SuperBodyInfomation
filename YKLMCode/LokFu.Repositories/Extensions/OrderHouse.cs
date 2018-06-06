using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class OrderHouse
    {
        private string cols = "Id,OId,HouseOwner,Mobile,Amoney";
        private string token;
        private string orderaddress;
        private string x;
        private string y;
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public string Token
        {
            get { return token; }
            set { token = value; }
        }
        public string OrderAddress
        {
            get { return orderaddress; }
            set { orderaddress = value; }
        }
        public string X
        {
            get { return x; }
            set { x = value; }
        }
        public string Y
        {
            get { return y; }
            set { y = value; }
        }

    }
}
