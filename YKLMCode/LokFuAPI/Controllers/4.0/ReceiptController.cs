using LokFu.Extensions;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LokFu.Controllers
{
    /// <summary>
    /// 收款接口(不再使用)
    /// </summary>
    public class ReceiptController : InitController
    {
        private string[] AllowTag = new string[] { "Alipay", "NFC", "Recharge", "RecMoneyLocal", "RecMoneyMulti", "WeiXin", "Transfer" };

        public ReceiptController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }

        public void Post()
        {
            DataObj.Data = @"{""userpay"":[{""cost"":""0.007"",""config"":{""shouyintai"":""0"",""zhitongche"":""1"",""yaoyiyao"":""1"",""shanhuzixuan"":""0""},""tag"":""WeiXin"",""cname"":""大额微信收款"",""state"":""0"",""snum"":2,""enum"":50000,""payway"":9}]}";
            DataObj.Code = "0000";
            DataObj.OutString();
            return;
            
            string Data = DataObj.GetData();
            if (!Data.IsNullOrEmpty())
            {
                JObject json = new JObject();
                try
                {
                    json = (JObject)JsonConvert.DeserializeObject(Data);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Receipt]:", "【Data】" + Data, Ex);
                }
                if (json == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                var Users = new Users();
                Users = JsonToObject.ConvertJsonToModel(Users, json);
                Users BaseUsers = Entity.Users.FirstOrDefault(o => o.Token == Users.Token);
                if (BaseUsers == null)//用户令牌不存在
                {
                    DataObj.OutError("2004");
                    return;
                }
                if (BaseUsers.State != 1)//用户被锁定
                {
                    DataObj.OutError("2003");
                    return;
                }
                if (BaseUsers.CardStae != 2)//未实名认证
                {
                    DataObj.OutError("2006");
                    return;
                }
                //if (BaseUsers.MiBao != 1)//未设置支付密码
                //{
                //    DataObj.OutError("2008");
                //    return;
                //}

                var result = new ReceiptModel();
                SysSet SysSet = Entity.SysSet.FirstOrDefault();
                ReceiptConfigModel ReceiptConfigModel = new ReceiptConfigModel()
                {
                    ShanHuZiXuan = SysSet.ShanHuZiXuan,
                    ShouYinTai = SysSet.ShouYinTai,
                    ZhiTongChe = SysSet.ZhiTongChe,
                };
                if (Equipment.RqType == "Apple")
                {
                    ReceiptConfigModel.YaoYiYao = SysSet.IosSet7;
                }
                if (Equipment.RqType == "Android")
                {
                    ReceiptConfigModel.YaoYiYao = SysSet.ApkSet7;
                }

                IList<SysControl> SysControlList = Entity.SysControl.Where(o => AllowTag.Contains(o.Tag) && (o.State == 1 || o.State == 2) && o.LagEntryDay==0).OrderBy(n => n.Sort).ToList();//SysControl
                IList<UserPay> UserPayList = Entity.UserPay.Where(n => n.UId == BaseUsers.Id).ToList();
                foreach (var p in SysControlList)
                {
                    p.Cols = "Tag,CName,State,SNum,ENum,PayWay,Cost,Config";
                    p.ChkState();
                    p.Cost = UserPayList.Where(o=>o.PId == p.PayWay).Select(o=>o.Cost).FirstOrNew();
                    if (ReceiptConfigModel.ShanHuZiXuan == 1 && p.IsPay == 1)
                    {
                        ReceiptConfigModel.ShanHuZiXuan = 1;
                    }else{
                        ReceiptConfigModel.ShanHuZiXuan = 0;
                    }
                    string JsStr = ReceiptConfigModel.OutJson();
                    JObject JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                    p.Config = JS;
                }
                result.UserPay = SysControlList.EntityToJson();
                
                string data = result.OutJson();
                data = data.Replace("\"[{", "[{").Replace("}]\"", "}]");
                DataObj.Data = data;
                DataObj.Code = "0000";
                DataObj.OutString();
                //Tools.OutString(ErrInfo.Return("0000"));
            }
        }


        public class ReceiptModel
        {
            private string cols = "UserPay";

            private string userpay;

            public string Cols
            {
                get { return cols; }
                set { cols = value; }
            }
            public string UserPay
            {
                get { return userpay; }
                set { userpay = value; }
            }
            
        }

        public class ReceiptConfigModel
        {
            private string cols = "YaoYiYao,ZhiTongChe,ShouYinTai,ShanHuZiXuan";
            private byte yaoyiyao;
            private byte zhitongche;
            private byte shouyintai;
            private byte shanhuzixuan;

            public string Cols
            {
                get { return cols; }
                set { cols = value; }
            }

            public byte ShouYinTai
            {
                get { return shouyintai; }
                set { shouyintai = value; }
            }

            public byte ZhiTongChe
            {
                get { return zhitongche; }
                set { zhitongche = value; }
            }

            public byte YaoYiYao
            {
                get { return yaoyiyao; }
                set { yaoyiyao = value; }
            }

            public byte ShanHuZiXuan
            {
                get { return shanhuzixuan; }
                set { shanhuzixuan = value; }
            }
        }
    }

   
}
