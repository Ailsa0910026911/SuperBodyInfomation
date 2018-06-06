using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class YYDevice
    {
        private string cols = "DevId,UUID,Major,Minor,ClickPV,ClickUV,ShakePV,ShakeUV";
        private int clickpv;
        private int clickuv;
        private int shakepv;
        private int shakeuv;
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public int ClickPV
        {
            get { return clickpv; }
            set { clickpv = value; }
        }
        public int ClickUV
        {
            get { return clickuv; }
            set { clickuv = value; }
        }
        public int ShakePV
        {
            get { return shakepv; }
            set { shakepv = value; }
        }
        public int ShakeUV
        {
            get { return shakeuv; }
            set { shakeuv = value; }
        }
    }
}