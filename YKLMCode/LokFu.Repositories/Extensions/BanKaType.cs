using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class BanKaType
    {
        private string cols = "Id,Title,Amoney,Pic,PayState";
        private string token;
        private byte paystate;

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
        public byte PayState
        {
            get { return paystate; }
            set { paystate = value; }
        }

    }
}