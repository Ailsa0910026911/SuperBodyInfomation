using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class JobOrders
    {
        private string cols = "TNum,TotalMoney,Amount,Poundage,State,AddTime";
        private string items;
        private string token;
        private string moblie;
        private int cardid;
        private string code;
        private int pg;
        private int pgs;

        public JObject UsersCard { get; set; }
        
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public int Pg
        {
            get { return pg; }
            set { pg = value; }
        }
        public int Pgs
        {
            get { return pgs; }
            set { pgs = value; }
        }
        public string Items
        {
            get { return items; }
            set { items = value; }
        }
        public string Token
        {
            get { return token; }
            set { token = value; }
        }

        public string Moblie
        {
            get { return moblie; }
            set { moblie = value; }
        }

        public int CardId
        {
            get { return cardid; }
            set { cardid = value; }
        }

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public string XPayMoney { get; set; }
        public string HPayMoney { get; set; }

        public int XFCount{ get; set; }

        public decimal SetCost { get; set; }

        public decimal SetCash { get; set; }


    }
}