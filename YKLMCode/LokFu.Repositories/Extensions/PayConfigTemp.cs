using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class PayConfigTemp
    {
        private string cols = "Id,PCCId,PId,Cost,Name";
        private string name = "";
        private string grouptype;
        private decimal poundage;
        
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string GroupType
        {
            get { return grouptype; }
            set { grouptype = value; }
        }

        public decimal Poundage
        {
            get { return poundage; }
            set { poundage = value; }
        }
    }
}
