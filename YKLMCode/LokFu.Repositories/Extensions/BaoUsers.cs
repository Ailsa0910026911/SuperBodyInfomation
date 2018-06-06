using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class BaoUsers
    {
        private string cols = "AllMoney,AllRec,LastRec,GetCost,YearPer,Alert";
        private string token;
        private string paypwd;
        private decimal getcost;
        private decimal yearper;
        private string alert;
        

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
        public string PayPwd
        {
            get { return paypwd; }
            set { paypwd = value; }
        }
        public decimal GetCost
        {
            get { return getcost; }
            set { getcost = value; }
        }
        public decimal YearPer
        {
            get { return yearper; }
            set { yearper = value; }
        }
        public string Alert
        {
            get { return alert; }
            set { alert = value; }
        }
    }
}
