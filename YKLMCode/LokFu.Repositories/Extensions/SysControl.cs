using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class SysControl
    {
        private string cols = "Name,ApkVer,ApkInt,ApkUrl,ApkInfo,IosVer,IosInt,IosUrl,IosInfo,APKState,IOSState";

        private double cost;

        private JObject config;

        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }

        public double Cost
        {
            get { return cost; }
            set { cost = value; }
        }

        public JObject Config
        {
            get { return config; }
            set { config = value; }
        }
    }
}
