using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class UserPay
    {
        private string cols = "Id,PId,Cost";
        private string token;
        private string name;

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
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

    }
}
