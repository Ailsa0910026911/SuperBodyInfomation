using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class BaoLog
    {
        private string cols = "Id,LType,LTypeName,Amount,AddTime";
        private string token;
        private int pg;
        private int pgs;
        private string ltypename;

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
        public string LTypeName
        {
            get { return ltypename; }
            set { ltypename = value; }
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
    }
}
