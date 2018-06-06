using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class ShareTotal
    {
        private string cols = "ShareNum,Amount,Profit,Tier";
        private string token;
        private decimal total = 0;
        private decimal today = 0;
        private decimal yesterday = 0;
        private JArray json;//json值
        private int usertype = 1;
        private int usertotal = 0;
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
        public decimal Total
        {
            get { return total; }
            set { total = value; }
        }
        public decimal Today
        {
            get { return today; }
            set { today = value; }
        }
        public decimal Yesterday
        {
            get { return yesterday; }
            set { yesterday = value; }
        }
        public JArray Json
        {
            get { return json; }
            set { json = value; }
        }
        public int UserType
        {
            get { return usertype; }
            set { usertype = value; }
        }
        public int UserTotal
        {
            get { return usertotal; }
            set { usertotal = value; }
        }
   

    }
}
