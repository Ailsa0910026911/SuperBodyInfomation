using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using LokFu.Extensions;

namespace LokFu.Controllers
{
    public class UserCardDeleteController : InitController
    {
        public UserCardDeleteController()
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
                Log.Write("[UserCardDelete]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserCard UserCard = new UserCard();
            UserCard = JsonToObject.ConvertJsonToModel(UserCard, json);
            //获取用户信息
            string Token = UserCard.Token;

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Token);
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

            UserCard = Entity.UserCard.FirstOrDefault(n => n.Id == UserCard.Id && n.UId == baseUsers.Id && n.State == 1);
            if (UserCard == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }
            if (UserCard.Type == 1){
                //借记卡需要判断是否用于一户一码
                FastUser FastUser = Entity.FastUser.FirstOrDefault(o => o.UId == baseUsers.Id);
                if (FastUser != null)
                {
                    if (!FastUser.Card.IsNullOrEmpty())
                    {
                        if (FastUser.Card == UserCard.Card)
                        {
                            DataObj.OutError("1105");
                            return;
                        }
                    }
                }
            }
            if (UserCard.Type == 2)
            {
                //借记卡需要判断是否用还款任务
                //任务正在使用的银行卡不能删除
                JobOrders JobOrders = Entity.JobOrders.Where(o => o.UId == baseUsers.Id && (o.State == 2 || o.State == 3) && o.UserCardId == UserCard.Id).FirstOrDefault();
                if (JobOrders != null)
                {
                    DataObj.Msg = "您的还款任务正在使用本银行卡，暂不能删除";
                    DataObj.OutError("7004");
                    return;
                }
                //24小时内有还款失败
                DateTime T24 = DateTime.Now.AddHours(-24);
                JobItem JobItem = Entity.JobItem.Where(o => o.UId == baseUsers.Id && o.State == 4 && o.UserCardId == UserCard.Id && o.RunTime > T24).FirstOrDefault();
                if (JobItem != null)
                {
                    DataObj.Msg = "最近1天您本银行卡有执行失败的还款任务，请隔1天再解除绑卡。";
                    DataObj.OutError("7004");
                    return;
                }
                //这里需要增加处理，是否开通授权，是的话需要把授权设置为0 
                //这里无法取得是哪个通道，系统每张卡只能绑定一个通道，所以只要把正常的关掉就可以了-Lin
                Entity.ExecuteStoreCommand("Update UserCardOpen Set State=0 Where UId=" + baseUsers.Id + " and CardNum = '" + UserCard.Card + "' And State=1");
            }
            UserCard.State = 0;
            UserCard.UnBindingTime = DateTime.Now;
            Entity.SaveChanges();

            //Entity.UserCard.DeleteObject(UserCard);
            //string SQL = "Update UserCard Set IsDel=1 Where Id=" + UserCard.Id;
            //Entity.ExecuteStoreCommand(SQL);
            //Entity.SaveChanges();

            //if (HasCache)
            //{
            //    string CashName = "UserCard_" + UserCard.Type + "_" + Token;
            //    CacheBuilder.EntityCache.Remove(CashName, null);
            //}

            DataObj.Data = UserCard.ToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
