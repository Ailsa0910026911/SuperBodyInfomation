using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class BanKaOrder
    {
        private string cols = "OId,Amoney,PayState";
        private string token;
        private string paypwd;

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
        public string PayPWD
        {
            get { return paypwd; }
            set { paypwd = value; }
        }
    }
}
