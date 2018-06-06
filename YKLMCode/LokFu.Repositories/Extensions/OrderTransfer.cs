using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class OrderTransfer
    {
        private string cols = "Id,OId,Bank,Owner,Amoney";
        private string token;
        private string tousername;
        private string mobile;
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
        public string Mobile
        {
            get { return mobile; }
            set { mobile = value; }
        }
        public string ToUserName
        {
            get { return tousername; }
            set { tousername = value; }
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
