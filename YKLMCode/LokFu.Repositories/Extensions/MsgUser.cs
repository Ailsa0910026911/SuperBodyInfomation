using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class MsgUser
    {
        private string cols = "Id,Name,Info,State,AddTime";
        private int pg;
        private int pgs;
        private string token;
        private string delete;

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
        public string Delete
        {
            get { return delete; }
            set { delete = value; }
        }

    }
}