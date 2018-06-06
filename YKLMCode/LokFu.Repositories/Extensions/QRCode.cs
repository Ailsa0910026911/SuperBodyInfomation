using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class QRCode
    {
        private string cols = "Id,Num,EditTime,State,UrlPam";
        private string token;
        private string urlpam;

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

        public string UrlPam
        {
            get { return urlpam; }
            set { urlpam = value; }
        }
    }
}
