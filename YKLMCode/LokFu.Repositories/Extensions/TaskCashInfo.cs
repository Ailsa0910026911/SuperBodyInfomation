﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class TaskCashInfo
    {
        private string cols = "Id,OId,TId";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}