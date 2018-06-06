using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LokFu.Repositories
{
    public partial class Card
    {
        private string token;
        public string Token
        {
            get { return token; }
            set { token = value; }
        }
    }
}
