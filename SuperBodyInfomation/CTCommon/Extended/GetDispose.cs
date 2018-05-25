using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTCommon.Extended
{
    public class GetDispose
    {
        public static string GetPoint(decimal num)
        {
            var num1 = num.ToString();
            var nl = num1.IndexOf(".");
            var num2 = num1.Substring(0,nl);
            return num2;
        }
    }
}
