using System;

namespace GoodPayJobs
{
    public static class Log
    {
        public static void Write(string Text, Exception Ex, string ext = "")
        {
            try
            {
                string FilePath = System.AppDomain.CurrentDomain.BaseDirectory;
                string filename = DateTime.Now.ToString("yyyyMMdd");
                string file = FilePath + "log\\" + "err_" + filename + ext + ".log";
                System.IO.StreamWriter log = new System.IO.StreamWriter(file, true);
                log.WriteLine("=============================================================================");
                log.WriteLine("TIME:" + System.DateTime.Now.ToLongTimeString());
                log.WriteLine("Text:" + Text);
                string ErrInfos = "null";
                if (Ex != null) ErrInfos = Ex.ToString();
                log.WriteLine("ErrInfo:" + ErrInfos);
                log.Close();
            }
            catch (Exception) {
                Write(Text, Ex, "_Ex");
            }
        }
        public static void Write(string Text, string ext = "")
        {
            try
            {
                string FilePath = System.AppDomain.CurrentDomain.BaseDirectory;
                string filename = DateTime.Now.ToString("yyyyMMdd");
                string file = FilePath + "log\\" + "log_" + filename + ext + ".log";
                System.IO.StreamWriter log = new System.IO.StreamWriter(file, true);
                log.WriteLine("=============================================================================");
                log.WriteLine("TIME:" + System.DateTime.Now.ToLongTimeString());
                log.WriteLine("Text:" + Text);
                log.Close();
            }
            catch (Exception) {
                Write(Text, "_Ex");
            }
        }
        public static void WriteLog(string Text, string fileExt, string ext = "")
        {
            try
            {
                string FilePath = System.AppDomain.CurrentDomain.BaseDirectory;
                string filename = DateTime.Now.ToString("yyyyMMdd");
                string file = FilePath + "log\\" + fileExt + "_" + filename + ext + ".log";
                System.IO.StreamWriter log = new System.IO.StreamWriter(file, true);
                log.WriteLine("=============================================================================");
                log.WriteLine("TIME:" + System.DateTime.Now.ToLongTimeString());
                log.WriteLine("Text:" + Text);
                log.Close();
            }
            catch (Exception)
            {
                WriteLog(Text, "_Ex");
            }
        }
    }
}
