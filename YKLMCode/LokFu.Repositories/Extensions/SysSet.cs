using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class SysSet
    {
        private string cols = "Name,ApkVer,ApkInt,ApkUrl,ApkInfo,IosVer,IosInt,IosUrl,IosInfo,APKState,IOSState,Home,More,Bottom";
        private int ist0 = 0;
        private string ctrlset = "";
        private string more = "";
        private string home = "";
        private string bottom = "";
        private string t0word = "";
        private string t1word = "";
        private JArray button;
        private int authtimes = 5;

        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public int isT0
        {
            get { return ist0; }
            set { ist0 = value; }
        }
        public string CtrlSet
        {
            get { return ctrlset; }
            set { ctrlset = value; }
        }
        public string Home
        {
            get { return home; }
            set { home = value; }
        }
        public string More
        {
            get { return more; }
            set { more = value; }
        }
        public string Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }
        public string T0Word
        {
            get { return t0word; }
            set { t0word = value; }
        }
        public string T1Word
        {
            get { return t1word; }
            set { t1word = value; }
        }

        public JArray Button
        {
            get { return button; }
            set { button = value; }
        }

        public int AuthTimes
        {
            get { return authtimes; }
            set { authtimes = value; }
        }
        
    }
}
