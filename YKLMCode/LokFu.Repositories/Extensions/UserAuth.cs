using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class UserAuth
    {
        private string cols = "OId,RetCode,RetMsg";
        private string token;
        private string cardnum;
        private string cardpwd;
        private string neekname;

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
        public string CardNum
        {
            get { return cardnum; }
            set { cardnum = value; }
        }
        public string CardPWD
        {
            get { return cardpwd; }
            set { cardpwd = value; }
        }
        public string NeekName
        {
            get { return neekname; }
            set { neekname = value; }
        }

    }
}