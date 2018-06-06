using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class Orders
    {
        private string cols = "Id,TNum,TName,Amoney,TState,TType,PayState,PayWay,PayTime,AddTime,Remark,IdCardState,UserCardPic,UserCardId,BankCardId,CardAddTime,CardEditTime,Poundage,LagEntryDay,StateTxt,Colour,Otypename";
        private string token;
        private string paypwd;
        private string payid;
        private int pg;
        private int pgs;
        private DateTime stime;
        private DateTime etime;
        private JObject json;//json值
        private string statetxt;
        private JArray piclist;
        private JArray payconfig;
        private string colour = string.Empty;
        private string otypename = string.Empty;

        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public string Token
        {
            get { return token; }
            set { token = value; }
        }
        public string PayPWD
        {
            get { return paypwd; }
            set { paypwd = value; }
        }
        public string PayId
        {
            get { return payid; }
            set { payid = value; }
        }
        public int Pg
        {
            get { return pg; }
            set { pg = value; }
        }
        public int Pgs
        {
            get { return pgs; }
            set { pgs = value; }
        }
        public DateTime STime
        {
            get { return stime; }
            set { stime = value; }
        }
        public DateTime ETime
        {
            get { return etime; }
            set { etime = value; }
        }
        public JObject Json {
            get { return json; }
            set { json = value; }
        }
        public string StateTxt {
            get { return statetxt; }
            set { statetxt = value; }
        }
        public JArray PayConfig
        {
            get { return payconfig; }
            set { payconfig = value; }
        }

        public JArray PicList
        {
            get { return piclist; }
            set { piclist = value; }
        }

        public string Colour
        {
            get { return colour; }
            set { colour = value; }
        }

        public string Otypename
        {
            get { return otypename; }
            set { otypename = value; }
        }

    }
}
