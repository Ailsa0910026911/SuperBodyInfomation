
namespace LokFu.Repositories
{
    public partial class SysAdmin
    {
        private string cols = "QQNum,QQName,QQState";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
    }
}
