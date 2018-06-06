using LokFu.Repositories;
using LokFu.Extensions;
namespace PC29.Base
{
    public static partial class SysAgentExtensions
    {
        /// <summary>
        /// 直通车交易获取代理的对应的分润比例
        /// </summary>
        /// <param name="SL"></param>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public static decimal GetSplit(this SysAgent SysAgent, int tier, LokFuEntity Entity)
        {
            decimal Split = 0;
            SysMoneySet SysMoneySet=Entity.SysMoneySet.FirstOrNew();
            if (tier == 1)
            {
                Split = SysMoneySet.PaySplitA1;
            }
            else if (tier == 2)
            {
                Split = SysMoneySet.PaySplitA2;
            }
            else if (tier == 3)
            {
                Split = SysMoneySet.PaySplitA3;
            }
            else if (tier == 4)
            {
                Split = SysMoneySet.PaySplitA4;
            }
            else if (tier == 5)
            {
                Split = SysMoneySet.PaySplitA5;
            }
            else if (tier == 6)
            {
                Split = SysMoneySet.PaySplitA6;
            }
            return Split;
        }

        /// <summary>
        /// 卡管家交易获取代理的对应的分润比例
        /// </summary>
        /// <param name="SL"></param>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public static decimal GetJobSplit(this SysAgent SysAgent, int tier, LokFuEntity Entity)
        {
            decimal Split = 0;
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            if (tier == 1)
            {
                Split = SysMoneySet.JobSplitA1;
            }
            else if (tier == 2)
            {
                Split = SysMoneySet.JobSplitA2;
            }
            else if (tier == 3)
            {
                Split = SysMoneySet.JobSplitA3;
            }
            else if (tier == 4)
            {
                Split = SysMoneySet.JobSplitA4;
            }
            else if (tier == 5)
            {
                Split = SysMoneySet.JobSplitA5;
            }
            else if (tier == 6)
            {
                Split = SysMoneySet.JobSplitA6;
            }
            return Split;
        }

        /// <summary>
        /// 开通代理获取代理的对应的分润比例
        /// </summary>
        /// <param name="tier">用户对应的相差层级</param>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public static decimal GetAgentSplit(this SysAgent SysAgent, int tier, LokFuEntity Entity)
        {
            decimal Split = 0;
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            if (tier == 1)
            {
                Split = SysMoneySet.AgentSplit0;
            }
            else if (tier == 2)
            {
                Split = SysMoneySet.AgentSplit1;
            }
            else if (tier == 3)
            {
                Split = SysMoneySet.AgentSplit2;
            }
            else if (tier == 4)
            {
                Split = SysMoneySet.AgentSplit3;
            }
            else if (tier == 5)
            {
                Split = SysMoneySet.AgentSplit4;
            }
            else if (tier == 6)
            {
                Split = SysMoneySet.AgentSplit5;
            }
            return Split;
        }

        /// <summary>
        /// 升级VIP获取代理的对应的分润
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public static decimal GetVIPSplit(this SysAgent SysAgent, int tier, LokFuEntity Entity)
        {
            decimal Split = 0;
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            if (tier == 1)
            {
                Split = SysMoneySet.VipSplitA1;
            }
            else if (tier == 2)
            {
                Split = SysMoneySet.VipSplitA2;
            }
            else if (tier == 3)
            {
                Split = SysMoneySet.VipSplitA3;
            }
            else if (tier == 4)
            {
                Split = SysMoneySet.VipSplitA4;
            }
            else if (tier == 5)
            {
                Split = SysMoneySet.VipSplitA5;
            }
            else if (tier == 6)
            {
                Split = SysMoneySet.VipSplitA6;
            }
            return Split;
        }
    }
}
