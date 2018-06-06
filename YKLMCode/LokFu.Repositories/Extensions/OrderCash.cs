using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class OrderCash
    {
        private string cols = "Id,OId,Bank,Owner,Amoney";
        private string token;
        private string orderaddress;
        private string x;
        private string y;
        private string paypwd;
        private decimal paymoney;
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
        public string PayPwd
        {
            get { return paypwd; }
            set { paypwd = value; }
        }
        public decimal PayMoney
        {
            get { return paymoney; }
            set { paymoney = value; }
        }

    }
}
