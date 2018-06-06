using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class FastOrder
    {
        private string cols = "TNum,OType,Amoney,Poundage,State,AddTime,Bank,Card,OTypeName,StateName,CashType,UserTime,Colour";
        private string token;
        private int pg;
        private int pgs;
        private DateTime stime;
        private DateTime etime;
        private string otypename = string.Empty;
        private string statename = string.Empty;
        private string colour = string.Empty;

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

        public string OTypeName
        {
            get
            {
                if (otypename == string.Empty)
                {
                    if (OType == 1)
                    {
                        return "支付宝";
                    }
                    else if (OType == 2)
                    {
                        return "微信";
                    }
                    else if (OType == 3)
                    {
                        return "银联";
                    }
                    else
                    {
                        return "未知";
                    }
                }
                else
                {
                    return otypename;
                }
            }
            set { otypename = value; }
        }
        public string StateName
        {
            get { return statename; }
            set { statename = value; }
        }

        public string Colour
        {
            get { return colour; }
            set { colour = value; }
        }
    }
}
