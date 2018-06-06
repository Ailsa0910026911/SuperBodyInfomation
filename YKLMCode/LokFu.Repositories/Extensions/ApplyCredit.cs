using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class ApplyCredit
    {
        private string cols = "Id,TrueName,BankId,BankName,MyScore,State,AddTime";
        private int pg;
        private int pgs;
        private string token;
        private string bankname;
        private DateTime stime;
        private DateTime etime;

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
        public string Token
        {
            get { return token; }
            set { token = value; }
        }
        public string BankName
        {
            get { return bankname; }
            set { bankname = value; }
        }
        public DateTime STime
        {
            get { return stime; }
            set { stime = value; }
        }
        public DateTime ETime
        {
            get { return etime; }
            set { etime = value; }
        }
    }
}