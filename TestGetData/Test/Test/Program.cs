using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string str = SendHttpPost("http://www.baidu.com","");
            Console.WriteLine(str);
            Console.ReadKey();
        }
        public static string SendHttpPost(string url,string paraJsonStr)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type","application/x-www-form-urlencoded");
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(paraJsonStr);
            byte[] responseData = webClient.UploadData(url,"Post",postData);
            string returnStr = System.Text.Encoding.UTF8.GetString(responseData);
            return returnStr;

        }
    }
}
