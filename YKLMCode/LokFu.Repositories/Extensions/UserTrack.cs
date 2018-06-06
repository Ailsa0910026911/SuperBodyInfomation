using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class UserTrack
    {
        private string cols = "";
        private string token;
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
        public string X { get; set; }
        public string Y { get; set; }
        public string RegAddress { get; set; }
    }
}