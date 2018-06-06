using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class OrderF2F
    {
        private string cols = "Id,OId,Amoney";
        private string token;
        private string orderaddress;
        private string x;
        private string y;
        private string ip;
        private string action;
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
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }
        public string Action
        {
            get { return action; }
            set { action = value; }
        }
        public int InType {
            get { return intype; }
            set { intype = value; }
        }
    }
}
