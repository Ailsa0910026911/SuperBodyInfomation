using LokFu.Extensions;
using LokFu.Repositories;
namespace PC29.Base
{
    public static partial class UsersExtensions
    {
        //直通车交易分润
        public static decimal GetUsersSplit(this Users U, LokFuEntity Entity, int Tier)
        {
            decimal split = 0;
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            if (Tier == 1)
            {
                split = SysMoneySet.PaySplitU0;
            }
            else if (Tier == 2)
            {
                split = SysMoneySet.PaySplitU1;
            }
            else if (Tier == 3)
            {
                split = SysMoneySet.PaySplitU2;
            }
            return split;
        }

        //卡管家交易分润
        public static decimal GetUsersJobSplit(this Users U, LokFuEntity Entity, int Tier)
        {
            decimal split = 0;
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            if (Tier == 1)
            {
                split = SysMoneySet.JobSplitU0;
            }
            else if (Tier == 2)
            {
                split = SysMoneySet.JobSplitU1;
            }
            else if (Tier == 3)
            {
                split = SysMoneySet.JobSplitU2;
            }
            return split;
        }

        public static decimal GetVIPSplit(this Users U, LokFuEntity Entity, int Tier)
        {
            decimal split = 0;
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            if (Tier == 1)
            {
                split = SysMoneySet.VipSplitU0;
            }
            else if (Tier == 2)
            {
                split = SysMoneySet.VipSplitU1;
            }
            else if (Tier == 3)
            {
                split = SysMoneySet.VipSplitU2;
            }
            return split;
        }

    }
}
