﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class FastPayWay
    {
        private string cols = "SNum,ENum,GroupType";
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
    }
}