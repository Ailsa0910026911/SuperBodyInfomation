using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class Users
    {
        private string cols = "Id,UserName,TrueName";
        private string cardpwd;
        private string newpwd;
        private string code;
        private int banknum;
        private int msgcount;
        private byte tier;
        private byte logintype;
        private string agenttel;
        private string autobank;
        private string ptoken;
        private decimal getcost;
        private decimal yearper;
        private int isanewupimg;
        private int pg;
        private int pgs;
        private int usertype;
        private int usertotal;
        private int agenttotal;
        private decimal vipprice;
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public string CardPWD
        {
            get { return cardpwd; }
            set { cardpwd = value; }
        }
        public string NewPWD
        {
            get { return newpwd; }
            set { newpwd = value; }
        }
        public int BankNum
        {
            get { return banknum; }
            set { banknum = value; }
        }
        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        public int MsgCount
        {
            get { return msgcount; }
            set { msgcount = value; }
        }
        public byte Tier
        {
            get { return tier; }
            set { tier = value; }
        }
        public byte LoginType
        {
            get { return logintype; }
            set { logintype = value; }
        }
        public string AgentTel
        {
            get { return agenttel; }
            set { agenttel = value; }
        }
        /// <summary>
        /// 自动提现绑定银行卡卡号
        /// </summary>
        public string AutoBank
        {
            get { return autobank; }
            set { autobank = value; }
        }

        public string PToken
        {
            get { return ptoken; }
            set { ptoken = value; }
        }

        public decimal GetCost
        {
            get { return getcost; }
            set { getcost = value; }
        }

        public decimal YearPer 
        {
            get { return yearper; }
            set { yearper = value; }
        }
        /// <summary>
        /// 旧版自动认证是否需要重新上传身份证
        /// </summary>
        public int IsAnewUpImg 
        {
            get { return isanewupimg; }
            set { isanewupimg = value; }
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
        public int UserType
        {
            get { return usertype; }
            set { usertype = value; }
        }
        public int UserTotal
        {
            get { return usertotal; }
            set { usertotal = value; }
        }

        public int AgentTotal
        {
            get { return agenttotal; }
            set { agenttotal = value; }
        }
        public decimal VipPrice
        {
            get { return vipprice; }
            set { vipprice = value; }
        }
    }
}