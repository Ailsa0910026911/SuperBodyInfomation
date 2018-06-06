using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class MsgCallBack
    {
        private string cols = "Id,NeekName,Linker,Name,Info,Result,State,AddTime,EditTime";
        private int pg;
        private int pgs;
        private string token;

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
    }
}