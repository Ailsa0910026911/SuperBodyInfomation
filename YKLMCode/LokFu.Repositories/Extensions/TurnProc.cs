using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class TurnProc
    {
        private int index;
        private int stnum;
        private int ennum;

        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        public int StNum
        {
            get { return stnum; }
            set { stnum = value; }
        }
        public int EnNum
        {
            get { return ennum; }
            set { ennum = value; }
        }
    }
}