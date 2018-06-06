using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
     public partial class SysMoneySet
    {
         private string cols = "Id";
         private string imageurl = string.Empty;
         private string agentpricelist = string.Empty;
         private int width;
         private int height;
         public string Cols
         {
             get { return cols; }
             set { cols = value; }
         }
         public string ImageUrl
         {
             get { return imageurl; }
             set { imageurl = value; }
         }
         public string AgentPricesList
         {
             get { return agentpricelist; }
             set { agentpricelist = value; }
         }
         public int Width
         {
             get { return width; }
             set { width = value; }
         }
         public int Height
         {
             get { return height; }
             set { height = value; }
         }
        //public decimal JobSplitA1 { get; set; }
        //public decimal JobSplitA2 { get; set; }
        //public decimal JobSplitA3 { get; set; }
        //public decimal JobSplitA4 { get; set; }
        //public decimal JobSplitA5 { get; set; }
        //public decimal JobSplitA6 { get; set; }
        //public decimal JobSplitU0 { get; set; }
        //public decimal JobSplitU1 { get; set; }
        //public decimal JobSplitU2 { get; set; }
    }
}
