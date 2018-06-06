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


namespace LokFu.Controllers
{
    public class MsgCallBackController : InitController
    {
        public MsgCallBackController()
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
                Log.Write("[MsgCallBack]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            MsgCallBack MsgCallBack = new MsgCallBack();
            MsgCallBack = JsonToObject.ConvertJsonToModel(MsgCallBack, json);
            if (MsgCallBack.Name.IsNullOrEmpty()) {
                MsgCallBack.Name = "1.0";
            }
            if (MsgCallBack.Name != "2.0")
            {
                MsgCallBack.Name = "1.0";
            }

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == MsgCallBack.Token);
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

            EFPagingInfo<MsgCallBack> p = new EFPagingInfo<MsgCallBack>();
            if (MsgCallBack.Pg.IsNullOrEmpty()) { MsgCallBack.Pg = 1; }
            if (!MsgCallBack.Pg.IsNullOrEmpty()) { p.PageIndex = MsgCallBack.Pg; }
            if (!MsgCallBack.Pgs.IsNullOrEmpty()) { p.PageSize = MsgCallBack.Pgs; }

            p.SqlWhere.Add(f => f.UId == baseUsers.Id);
            p.SqlWhere.Add(f => f.State > 0);

            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgCallBack> List = Entity.Selects<MsgCallBack>(p);
            IList<MsgCallBack> iList = new List<MsgCallBack>();
            if (MsgCallBack.Name == "2.0")
            { //处理接口
                var i = 1;
                foreach (var item in List) {
                    if (item.State == 2) {
                        if (!item.Result.IsNullOrEmpty()) {
                            MsgCallBack Msg = new MsgCallBack();
                            Msg.Id = i;
                            Msg.Info = item.Result;
                            Msg.State = 2;
                            Msg.AddTime = (DateTime)item.EditTime;
                            Msg.Cols = "Info,State,AddTime";
                            iList.Add(Msg);
                            i++;
                        }
                    }
                    MsgCallBack msg = new MsgCallBack();
                    msg.Id = i;
                    msg.Info = item.Info;
                    msg.State = 1;
                    msg.AddTime = item.AddTime;
                    msg.Cols = "Info,State,AddTime";
                    iList.Add(msg);
                    i++;
                }
                if (List.TotalPage == MsgCallBack.Pg || List.TotalPage == 0)
                {
                    MsgCallBack msg = new MsgCallBack();
                    msg.Id = i;
                    msg.Info = "您好，很高兴为您服务！";
                    msg.State = 2;
                    msg.AddTime = DateTime.Now;
                    msg.Cols = "Info,State,AddTime";
                    iList.Add(msg);
                    i++;
                }
                iList = iList.OrderByDescending(n => n.Id).ToList();
            }
            else { 
                iList = List.ToList();
            }
            StringBuilder sb = new StringBuilder("");
            sb.Append("{");
            sb.Append(List.PageToString());
            sb.Append(",");
            sb.Append(iList.EntityToString());
            sb.Append("}");
            DataObj.Data = sb.ToString();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
