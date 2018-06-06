using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class AppUpdate
    {
        private string cols = "Name,Tag,ApkVer,ApkInt,ApkUrl,ApkYYB,ApkInfo,ApkColor,APKState,IosVer,IosInt,IosUrl,IosYYB,IosInfo,IOSState,IosColor";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}