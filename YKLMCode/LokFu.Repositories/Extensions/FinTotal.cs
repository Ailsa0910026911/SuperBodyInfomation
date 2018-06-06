using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class FinTotal
    {
        private string cols = "Id,Update,TotalAmoney,TotlaPoundage,Amoney1,Poundage1,Amoney2,Amoney2_0,Poundage2_0,Number2,Poundage2_1,Amoney3,Poundage3,Amoney5,Poundage5,Amoney5_0,Poundage5_0,Amoney6,Amoney7,Poundage7,Amoney8,Poundage8,Amoney9,Poundage9";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}
