using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
namespace LokFu.Controllers
{
    public class CancelJobController : InitController
    {
        public CancelJobController()
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
                Log.Write("[JobOrders]:", "【Data】" + Data, Ex);
                DataObj.OutError("1000");
                return;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            JobOrders JobOrders = new JobOrders();
            JobOrders = JsonToObject.ConvertJsonToModel(JobOrders, json);

            if (JobOrders.TNum.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }

            #region 获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == JobOrders.Token);
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
            #endregion
            Entity.ExecuteStoreCommand("Exec SP_JobReSet '" + JobOrders.TNum + "',1");
            JobOrders baseJobOrders = Entity.JobOrders.FirstOrNew(o => o.TNum == JobOrders.TNum && o.UId == baseUsers.Id);
            if (baseJobOrders == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (baseJobOrders.State != 3)
            {
                DataObj.Msg = "当前订单状态不能取消";
                DataObj.OutError("1000");
                return;
            }
            bool IsItemRun = Entity.JobItem.Any(o => o.TNum == baseJobOrders.TNum && o.State == 2);
            if (IsItemRun)
            {
                DataObj.Msg = "子订单有正在执行中的状态,不能执行该操作";
                DataObj.OutError("1000");
                return;
            }
            baseJobOrders.State = 5;
            baseJobOrders.Remark = "用户自主取消订单,时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Entity.SaveChanges();
            //取消所有待执行订单
            Entity.ExecuteStoreCommand("Update JobItem Set State=0 Where TNum='" + baseJobOrders.TNum + "' and State=1");

            //重新统计成功与失败后金额
            if (baseJobOrders.Amount >= 1)
            {
                DataObj = this.CloseJob(JobOrders.TNum);
            }
            DataObj.Data = "取消订单成功";
            DataObj.Code = "0000";
            DataObj.OutString();
        }

        private DataObj CloseJob(string tnum)
        {
            DataObj DataObj = new DataObj();
            if (tnum.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return DataObj;
            }
            //重新统计成功与失败后金额
            Entity.ExecuteStoreCommand("Exec SP_JobReSet '" + tnum + "',1");

            JobOrders baseJobOrders = Entity.JobOrders.FirstOrDefault(n => n.TNum == tnum);
            if (baseJobOrders == null)
            {
                DataObj.OutError("1000");
                return DataObj;
            }
            if (baseJobOrders.State != 5)
            {
                DataObj.Msg = "当前订单状态不能结清操作";
                DataObj.OutError("1000");
                return DataObj;
            }
            if (baseJobOrders.Amount < 1)
            {
                DataObj.Msg = "本订单余额小于0无需再次清算";
                DataObj.OutError("1000");
                return DataObj;
            }
            int count = Entity.JobItem.Count(n => n.TNum == tnum && n.State != 3 && n.State != 4 && n.State != 0);
            if (count > 0)
            {
                DataObj.Msg = "本订单有" + count + "笔交易未达到最终状态，暂不可操作。";
                DataObj.OutError("1000");
                return DataObj;
            }
            DateTime Now = DateTime.Now;
            DateTime RunTime = Now;
            if (RunTime.Hour < 12)
            {
                RunTime.AddHours(12 - RunTime.Hour);
            }
            //生成多一单还款计划就OK了
            JobItem JobItem = new JobItem();
            JobItem.UId = baseJobOrders.UId;
            JobItem.TNum = baseJobOrders.TNum;
            JobItem.RunMoney = baseJobOrders.Amount;


            JobItem.RunTime = RunTime;
            JobItem.Poundage = 0;
            JobItem.RunGet = JobItem.RunMoney * baseJobOrders.CashRate;
            if (JobItem.RunGet < baseJobOrders.CashMin)
            {
                JobItem.RunGet = baseJobOrders.CashMin;
            }
            if (JobItem.RunGet > baseJobOrders.CashMax)
            {
                JobItem.RunGet = baseJobOrders.CashMax;
            }
            JobItem.RunGet = JobItem.RunGet.Ceiling();//通道成本
            JobItem.AgentGet = 0;
            //利润=用户手续费-代理分润-通道成本
            JobItem.HFGet = JobItem.Poundage - JobItem.AgentGet - JobItem.RunGet;
            JobItem.State = 1;
            JobItem.AddTime = Now;
            JobItem.RunType = 2;
            JobItem.RunState = 0;
            JobItem.PayWay = baseJobOrders.CashWay;
            JobItem.UserCardId = baseJobOrders.UserCardId;
            JobItem.Remark = "任务失败退回剩余金额";
            JobItem.RunSort = 9999;
            Entity.JobItem.AddObject(JobItem);
            Entity.SaveChanges();
            DataObj.Code = "0000";
            return DataObj;
        }
    }
}
