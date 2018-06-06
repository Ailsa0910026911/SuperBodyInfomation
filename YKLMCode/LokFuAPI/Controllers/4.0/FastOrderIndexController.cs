using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LokFu.Extensions;
using LokFu.FastPay;
using System.Web.Script.Serialization;


namespace LokFu.Controllers
{
    public class FastOrderIndexController : InitController
    {
        public FastOrderIndexController()
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
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[FastOrders]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }

            var model = new FastOrderIndexModel();

            //获取直通车配置及用户直通车配置
            FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == baseUsers.Id);
            if (FastUser == null)
            {
                //开通帐户
                FastUser = new FastUser();
                FastUser.UId = baseUsers.Id;
                FastUser.TrueName = baseUsers.TrueName;
                FastUser.CardId = baseUsers.CardId;
                FastUser.AddTime = DateTime.Now;
                Entity.FastUser.AddObject(FastUser);
                Entity.SaveChanges();
            }

            if (FastUser.Card.IsNullOrEmpty())
            {
                //未绑卡
                IList<UserCard> UserCardList = Entity.UserCard.Where(n => n.UId == baseUsers.Id && n.Type == 1 && n.State == 1).ToList();
                if (UserCardList.Count < 1)
                {
                    DataObj.OutError("2099");
                    return;
                }
            }

            FastConfig FastConfig = Entity.FastConfig.FirstOrNew();
            //===========================================================
            //这里要处理商户入驻
            BusFastPay.AddMer(FastUser, FastConfig, this.Entity);
            //===========================================================

            var BasicBank = Entity.BasicBank.FirstOrNew(o => o.Name == FastUser.Bank);

            model.bank = FastUser.Bank ?? string.Empty;
            model.uid = FastUser.UId;
            model.card = FastUser.Card ?? string.Empty;
            model.bin = FastUser.Bin ?? string.Empty;
            model.bid = BasicBank.Id;
            model.caption1 = "使用收付直通车直接结算到银行卡,无需提现,交易手续费为0.49%+2元.";
            model.caption2 = "每天9:00-21:00实时到账,其它时间交易为T+1到账";

            DataObj.Data = JsonConvert.SerializeObject(model);
            DataObj.Code = "0000";
            DataObj.OutString();
        }

        public class FastOrderIndexModel
        {
            public int uid { get; set; }

            public string card { get; set; }

            public string bank { get; set; }

            public string bin { get; set; }

            public int bid { get; set; }

            public string caption1 { get; set; }

            public string caption2 { get; set; }
        }
    }
}
