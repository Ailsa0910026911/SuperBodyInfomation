﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class UserCard
    {
        private string cols = "Id,Bank,Card,Name,Type,Deposit,Province,City,District,Bin,BId,Mobile,BillDay,RefundDay";
        private string token;
        private string paypwd;

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
        public string PayPwd
        {
            get { return paypwd; }
            set { paypwd = value; }
        }
        public string ProvinceName { get; set; }
        public string CityName { get; set; }
        public string ValidateCode { get; set; }
        public bool IsRun { get; set; }
        public string TNum { get; set; }
    }
}