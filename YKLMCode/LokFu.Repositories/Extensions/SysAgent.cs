using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class SysAgent
    {
        private string cols = "Id,Name";
        public AgentType AgentType { get; set; }
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}