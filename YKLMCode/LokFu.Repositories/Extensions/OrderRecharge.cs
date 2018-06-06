using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class OrderRecharge
    {
        private string cols = "Id,OId,Amoney";
        private string token;
        private string orderaddress;
        private string x;
        private string y;
        private int intype;

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
        public int InType
        {
            get { return intype; }
            set { intype = value; }
        }


    }
}
