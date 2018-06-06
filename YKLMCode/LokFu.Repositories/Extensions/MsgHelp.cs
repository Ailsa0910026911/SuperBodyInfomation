using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LokFu.Repositories
{
    public partial class MsgHelp
    {
        private string cols = "Id,Name,Info,AddTime,Pic1,Pic2,Pic3,Click";
        private string token;
        private int pg;
        private int pgs;
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
    }
}
