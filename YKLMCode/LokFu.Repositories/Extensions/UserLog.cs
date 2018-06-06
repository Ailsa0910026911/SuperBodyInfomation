using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class UserLog
    {
        private string cols = "OId,OType,Amount,AfterAmount,AddTime,AfterFrozen,ProfitName";
        private string token;
        private string name;
        private int pg;
        private int pgs;
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
        public string Name
        {
            get { return name; }
            set { name = value; }
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
        public string ProfitName
        {
            get { return profitname; }
            set { profitname = value; }
        }
    }
}
