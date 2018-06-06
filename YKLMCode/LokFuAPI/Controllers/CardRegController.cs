using LokFu.Extensions;
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
using System.Web.Script.Serialization;
using LokFu.HFJS;
using LokFu.HFJS.HFJSModels;
using LokFu.HFJS.HFJSResults;
namespace LokFu.Controllers
{
    public class CardRegController : InitController
    {
        //
        // GET: /CardReg/

        public CardRegController()
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
                Log.Write("[CardReg]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Card Card = new Card();
            Card = JsonToObject.ConvertJsonToModel(Card, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Card.Token);
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
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }
            if (baseUsers.IsVip == 1)//是VIP不能再生成
            {
                DataObj.OutError("9001");
                return;
            }
            Card = Entity.Card.FirstOrDefault(n => n.Code == Card.Code && n.PasWd == Card.PasWd && n.Auto == 1);
            if (Card == null)
            {
                DataObj.OutError("5003");
                return;
            }
            if (Card.State != 1)
            {
                string StateStr = string.Empty;
                switch (Card.State)
                {
                    case 2:
                        StateStr = "已授权";
                        break;
                    case 3:
                        StateStr = "已使用";
                        break;
                    case 0:
                        StateStr = "已失效";
                        break;
                }

                DataObj.Msg = "激活码" + StateStr;
                DataObj.OutError("5003");
                return;
            }
            if (Card.AId.IsNullOrEmpty())
            {
                DataObj.OutError("5002");
                return;
            }
            if (Card.AdminId.IsNullOrEmpty())
            {
                DataObj.OutError("5002");
                return;
            }
            Card.State = 2;//使用中
            Card.PUId = baseUsers.Id;
            baseUsers.Agent = Card.AId;
            baseUsers.CardNum = Card.Code;
            baseUsers.IsVip = 1;

            //修改到app费率
            IList<UserPay> UserPayList = Entity.UserPay.Where(o => o.UId == baseUsers.Id).ToList();
            foreach (var temp in UserPayList)
            {
                if (temp.Cost > 0.005)
                {
                    temp.Cost = 0.005;
                }
            }
           // Entity.SaveChanges();


            //修改用户通道费率

            FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(o => o.UId == baseUsers.Id && o.BusiState == 1 && o.MerState == 1 && o.CardState == 1);
            if (FastUserPay != null)
            {
                FastPayWay FastPayWay = Entity.FastPayWay.FirstOrNew(o => o.State == 1);
                FastConfig FastConfig = Entity.FastConfig.FirstOrNew();
                decimal Cost = 0.005M;//费率
                decimal Cash = FastConfig.UserCash;//手续费
                string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
                string Code = PayConfigArr[0];
                string CodeKey = PayConfigArr[1];
                string PayWayCode = PayConfigArr[2];
                userspayopenbModel userspayopenbModel = new userspayopenbModel()
                {
                    merid = FastUserPay.MerId,
                    paywaycode = PayWayCode,
                    code = Code,
                    bankcost = Cost,
                    //surcharge = 0,
                    cash = Cash,
                    bankcostmin = 1.2M,
                    bankcostmax = 9999999
                };
                fastuserResult fastuserResult = HFJSTools.userspayedit(userspayopenbModel, CodeKey);
                if (fastuserResult.respcode == "00")
                {
                    if (fastuserResult.state == 1)
                    {
                        FastUserPay.BusiState = 1;
                        FastUserPay.BusiMsg = fastuserResult.respcode + "升级Vip[" + fastuserResult.respmsg + "]";
                        FastUserPay.UserCost3 = 0.005M;
                    }
                    else
                    {
                        FastUserPay.BusiState = 4;
                        FastUserPay.BusiMsg = fastuserResult.respcode + "升级Vip[" + fastuserResult.respmsg + "]";
                    }
                }
                else
                {
                    FastUserPay.BusiState = 4;
                    FastUserPay.BusiMsg = fastuserResult.respcode + "升级Vip[" + fastuserResult.respmsg + "]";
                }
            }
            Entity.SaveChanges();
            DataObj.Data = baseUsers.ToString();
            DataObj.Code = "0000";
            DataObj.OutString();
        }

    }
}
