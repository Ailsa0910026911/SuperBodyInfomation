using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class BasicBankCard
    {
        private string cols = "BankName,Name,CType,Length";
        private string bankname = "";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public string BankName
        {
            get { return bankname; }
            set { bankname = value; }
        }
    }
}