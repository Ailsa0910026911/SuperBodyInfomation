using LokFu.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LokFu.Controllers
{
    public class BizExt
    {
        /// <summary>
        /// 新旧版本判断 for IOS:8.0.3/安卓:8.0.6
        /// true:新版 false:旧版
        /// </summary>
        public static bool NewOrOldVersion(SysAgent SysAgent, Equipment Equipment, LokFuEntity Entity)
        {
            //处理贴牌相关
            bool result = false;
            var topSysAgent = SysAgent.GetTopAgent(Entity);

            if (!Equipment.SoftVer.IsNullOrEmpty())
            {
                Version v1 = new Version(Equipment.SoftVer);//当前版本
                Version v2 = new Version("1.0");

                if (Equipment.RqType.ToLower() == "apple")
                {
                    //苹果
                    if (topSysAgent.IsTeiPai == 0)//好付
                    {
                        v2 = new Version("8.0.3");
                    }
                    else//贴牌
                    {
                        v2 = new Version("8.0");
                    }

                }
                else if (Equipment.RqType.ToLower() == "android")
                {
                    //安卓
                    if (topSysAgent.IsTeiPai == 0)//好付
                    {
                        v2 = new Version("8.0.6");
                    }
                    else //贴牌
                    {
                        v2 = new Version("8.0.0");
                    }
                }
                if (v1 > v2)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}