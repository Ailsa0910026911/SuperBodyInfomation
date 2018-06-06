using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class OrderProfitLog
    {
        private string cols = "AddTime,TNum,Profit,UserName,OrderType,Amoney,ProfitName";
        private string token;
        private int pg;
        private int pgs;
        private DateTime stime;
        private DateTime etime;
        private string profitname;
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
        public Users users{get;set;}
        public SysAgent sysAgent{get;set;}
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
        public string ProfitName
        {
            get { return profitname; }
            set { profitname = value; }
        }
    }
}
