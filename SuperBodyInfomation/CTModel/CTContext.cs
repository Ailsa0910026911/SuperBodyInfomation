namespace CTModel
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CTContext : DbContext
    {
        public CTContext()
            : base("name=CTContext")
        {
        }

        public virtual DbSet<AdInfo> AdInfo { get; set; }
        public virtual DbSet<AdTag> AdTag { get; set; }
        public virtual DbSet<AgentType> AgentType { get; set; }
        public virtual DbSet<APPBlock> APPBlock { get; set; }
        public virtual DbSet<ApplyCredit> ApplyCredit { get; set; }
        public virtual DbSet<ApplyCreditCard> ApplyCreditCard { get; set; }
        public virtual DbSet<ApplyJoin> ApplyJoin { get; set; }
        public virtual DbSet<ApplyLoan> ApplyLoan { get; set; }
        public virtual DbSet<APPModule> APPModule { get; set; }
        public virtual DbSet<AppUpdate> AppUpdate { get; set; }
        public virtual DbSet<Attach> Attach { get; set; }
        public virtual DbSet<BanKaList> BanKaList { get; set; }
        public virtual DbSet<BanKaOrder> BanKaOrder { get; set; }
        public virtual DbSet<BanKaType> BanKaType { get; set; }
        public virtual DbSet<BankCodeList> BankCodeList { get; set; }
        public virtual DbSet<BaoConfig> BaoConfig { get; set; }
        public virtual DbSet<BaoLog> BaoLog { get; set; }
        public virtual DbSet<BaoStory> BaoStory { get; set; }
        public virtual DbSet<BaoUsers> BaoUsers { get; set; }
        public virtual DbSet<BasicBank> BasicBank { get; set; }
        public virtual DbSet<BasicBankCard> BasicBankCard { get; set; }
        public virtual DbSet<BasicBankInfo> BasicBankInfo { get; set; }
        public virtual DbSet<BasicCarBrand> BasicCarBrand { get; set; }
        public virtual DbSet<BasicCardBin> BasicCardBin { get; set; }
        public virtual DbSet<BasicCity> BasicCity { get; set; }
        public virtual DbSet<BasicCode> BasicCode { get; set; }
        public virtual DbSet<BasicDesc> BasicDesc { get; set; }
        public virtual DbSet<BasicDistrict> BasicDistrict { get; set; }
        public virtual DbSet<BasicMobile> BasicMobile { get; set; }
        public virtual DbSet<BasicProvince> BasicProvince { get; set; }
        public virtual DbSet<BusinessInfo> BusinessInfo { get; set; }
        public virtual DbSet<BusinessUsers> BusinessUsers { get; set; }
        public virtual DbSet<Card> Card { get; set; }
        public virtual DbSet<CheckResult> CheckResult { get; set; }
        public virtual DbSet<CheckTask> CheckTask { get; set; }
        public virtual DbSet<CheckUser> CheckUser { get; set; }
        public virtual DbSet<CheckUserMoney> CheckUserMoney { get; set; }
        public virtual DbSet<CutAct> CutAct { get; set; }
        public virtual DbSet<CutLog> CutLog { get; set; }
        public virtual DbSet<CutUsers> CutUsers { get; set; }
        public virtual DbSet<DaiLiApply> DaiLiApply { get; set; }
        public virtual DbSet<DaiLiOrder> DaiLiOrder { get; set; }
        public virtual DbSet<DB_Account_DailyCompared> DB_Account_DailyCompared { get; set; }
        public virtual DbSet<DB_Account_DailyStatistic> DB_Account_DailyStatistic { get; set; }
        public virtual DbSet<DeductMoney> DeductMoney { get; set; }
        public virtual DbSet<DownFile> DownFile { get; set; }
        public virtual DbSet<DownFileTag> DownFileTag { get; set; }
        public virtual DbSet<Equipment> Equipment { get; set; }
        public virtual DbSet<FastConfig> FastConfig { get; set; }
        public virtual DbSet<FastOrder> FastOrder { get; set; }
        public virtual DbSet<FastOrderChange> FastOrderChange { get; set; }
        public virtual DbSet<FastOrderOP> FastOrderOP { get; set; }
        public virtual DbSet<FastPayWay> FastPayWay { get; set; }
        public virtual DbSet<FastSplit> FastSplit { get; set; }
        public virtual DbSet<FastUser> FastUser { get; set; }
        public virtual DbSet<FastUserPay> FastUserPay { get; set; }
        public virtual DbSet<FinTotal> FinTotal { get; set; }
        public virtual DbSet<HaoWool> HaoWool { get; set; }
        public virtual DbSet<JobItem> JobItem { get; set; }
        public virtual DbSet<JobLog> JobLog { get; set; }
        public virtual DbSet<JobOrders> JobOrders { get; set; }
        public virtual DbSet<JobPayWay> JobPayWay { get; set; }
        public virtual DbSet<JobSet> JobSet { get; set; }
        public virtual DbSet<JobUserPay> JobUserPay { get; set; }
        public virtual DbSet<MsgAbout> MsgAbout { get; set; }
        public virtual DbSet<MsgAdmin> MsgAdmin { get; set; }
        public virtual DbSet<MsgCallBack> MsgCallBack { get; set; }
        public virtual DbSet<MsgHelp> MsgHelp { get; set; }
        public virtual DbSet<MsgNotice> MsgNotice { get; set; }
        public virtual DbSet<MsgTemplate> MsgTemplate { get; set; }
        public virtual DbSet<MsgUser> MsgUser { get; set; }
        public virtual DbSet<OrderCash> OrderCash { get; set; }
        public virtual DbSet<OrderCashLog> OrderCashLog { get; set; }
        public virtual DbSet<OrderF2F> OrderF2F { get; set; }
        public virtual DbSet<OrderHouse> OrderHouse { get; set; }
        public virtual DbSet<OrderProfitLog> OrderProfitLog { get; set; }
        public virtual DbSet<OrderRecharge> OrderRecharge { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<OrdersDDLog> OrdersDDLog { get; set; }
        public virtual DbSet<OrdersPayOnly> OrdersPayOnly { get; set; }
        public virtual DbSet<OrdersRec> OrdersRec { get; set; }
        public virtual DbSet<OrdersRefund> OrdersRefund { get; set; }
        public virtual DbSet<OrdersRefundLog> OrdersRefundLog { get; set; }
        public virtual DbSet<OrdersRepair> OrdersRepair { get; set; }
        public virtual DbSet<OrdersRepairLog> OrdersRepairLog { get; set; }
        public virtual DbSet<OrderTransfer> OrderTransfer { get; set; }
        public virtual DbSet<PayConfig> PayConfig { get; set; }
        public virtual DbSet<PayConfigChange> PayConfigChange { get; set; }
        public virtual DbSet<PayConfigOrder> PayConfigOrder { get; set; }
        public virtual DbSet<PayConfigTemp> PayConfigTemp { get; set; }
        public virtual DbSet<PayLog> PayLog { get; set; }
        public virtual DbSet<QRCode> QRCode { get; set; }
        public virtual DbSet<SalesLog> SalesLog { get; set; }
        public virtual DbSet<ShareTotal> ShareTotal { get; set; }
        public virtual DbSet<SMSCode> SMSCode { get; set; }
        public virtual DbSet<SMSLog> SMSLog { get; set; }
        public virtual DbSet<StopPayAudit> StopPayAudit { get; set; }
        public virtual DbSet<SysAdmin> SysAdmin { get; set; }
        public virtual DbSet<SysAgent> SysAgent { get; set; }
        public virtual DbSet<SysAgentSet> SysAgentSet { get; set; }
        public virtual DbSet<SysAsk> SysAsk { get; set; }
        public virtual DbSet<SysControl> SysControl { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<SysLog> SysLog { get; set; }
        public virtual DbSet<SysMenu> SysMenu { get; set; }
        public virtual DbSet<SysMoneySet> SysMoneySet { get; set; }
        public virtual DbSet<SysPower> SysPower { get; set; }
        public virtual DbSet<SysSet> SysSet { get; set; }
        public virtual DbSet<TaskCash> TaskCash { get; set; }
        public virtual DbSet<TaskCashInfo> TaskCashInfo { get; set; }
        public virtual DbSet<TaskOrders> TaskOrders { get; set; }
        public virtual DbSet<TaskTimeSet> TaskTimeSet { get; set; }
        public virtual DbSet<TurnLog> TurnLog { get; set; }
        public virtual DbSet<TurnProc> TurnProc { get; set; }
        public virtual DbSet<Turntable> Turntable { get; set; }
        public virtual DbSet<TurnUsers> TurnUsers { get; set; }
        public virtual DbSet<UserAsk> UserAsk { get; set; }
        public virtual DbSet<UserAuth> UserAuth { get; set; }
        public virtual DbSet<UserBlackList> UserBlackList { get; set; }
        public virtual DbSet<UserCard> UserCard { get; set; }
        public virtual DbSet<UserCardOpen> UserCardOpen { get; set; }
        public virtual DbSet<UserFrozenLog> UserFrozenLog { get; set; }
        public virtual DbSet<UserLog> UserLog { get; set; }
        public virtual DbSet<UserLoginSceneid> UserLoginSceneid { get; set; }
        public virtual DbSet<UserMaillist> UserMaillist { get; set; }
        public virtual DbSet<UserPay> UserPay { get; set; }
        public virtual DbSet<UserPayAgent> UserPayAgent { get; set; }
        public virtual DbSet<UserPayChange> UserPayChange { get; set; }
        public virtual DbSet<UserPayCredit> UserPayCredit { get; set; }
        public virtual DbSet<UserPayTemp> UserPayTemp { get; set; }
        public virtual DbSet<UserPic> UserPic { get; set; }
        public virtual DbSet<UserPromoteGet> UserPromoteGet { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<UsersAmountErrLog> UsersAmountErrLog { get; set; }
        public virtual DbSet<UsersFace> UsersFace { get; set; }
        public virtual DbSet<UsersMoveLog> UsersMoveLog { get; set; }
        public virtual DbSet<UsersPayCard> UsersPayCard { get; set; }
        public virtual DbSet<UsersPayCardOpen> UsersPayCardOpen { get; set; }
        public virtual DbSet<UserTrack> UserTrack { get; set; }
        public virtual DbSet<UserTrail> UserTrail { get; set; }
        public virtual DbSet<VIPOrder> VIPOrder { get; set; }
        public virtual DbSet<WeiXinUsers> WeiXinUsers { get; set; }
        public virtual DbSet<YYApply> YYApply { get; set; }
        public virtual DbSet<YYDaily> YYDaily { get; set; }
        public virtual DbSet<YYDevice> YYDevice { get; set; }
        public virtual DbSet<YYPage> YYPage { get; set; }
        public virtual DbSet<BusinessShareProfit> BusinessShareProfit { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AgentType>()
                .Property(e => e.RegisterFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ApplyCredit>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ApplyCredit>()
                .Property(e => e.AgentMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ApplyCredit>()
                .Property(e => e.AIdMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ApplyLoan>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ApplyLoan>()
                .Property(e => e.AgentMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ApplyLoan>()
                .Property(e => e.AIdMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BanKaOrder>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BanKaType>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoConfig>()
                .Property(e => e.GetCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoConfig>()
                .Property(e => e.YearPer)
                .HasPrecision(6, 4);

            modelBuilder.Entity<BaoLog>()
                .Property(e => e.BeforAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoLog>()
                .Property(e => e.BeforFrozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoLog>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoLog>()
                .Property(e => e.AfterAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoLog>()
                .Property(e => e.AfterFrozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.GetCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.YearPer)
                .HasPrecision(6, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.InMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.OutMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.BfAllMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.BfActMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.BfInMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.Interest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.AfAllMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.AfActMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoStory>()
                .Property(e => e.AfInMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoUsers>()
                .Property(e => e.AllMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoUsers>()
                .Property(e => e.ActMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoUsers>()
                .Property(e => e.InMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoUsers>()
                .Property(e => e.AllRec)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BaoUsers>()
                .Property(e => e.LastRec)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckResult>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUser>()
                .Property(e => e.PastMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUser>()
                .Property(e => e.PastFrozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUser>()
                .Property(e => e.CheckMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUser>()
                .Property(e => e.CheckFrozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUser>()
                .Property(e => e.NowMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUser>()
                .Property(e => e.NowFrozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUserMoney>()
                .Property(e => e.ChangeMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUserMoney>()
                .Property(e => e.ChangeFrozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUserMoney>()
                .Property(e => e.ProgressMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CheckUserMoney>()
                .Property(e => e.RecordMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CutLog>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CutUsers>()
                .Property(e => e.AllPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CutUsers>()
                .Property(e => e.MyPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CutUsers>()
                .Property(e => e.CutPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DaiLiApply>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DaiLiOrder>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DaiLiOrder>()
                .Property(e => e.SameGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_P1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_7)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_P7)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_8)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_P8)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_9)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_P9)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.Baglog)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.TurnLog)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.OrderProfitLog)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_3)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_P3)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.Userlog15)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_2)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_5)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_6)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.ORDERS_12)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.UserAuth)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.U_Amony)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.U_Frozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.B_Amony)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyCompared>()
                .Property(e => e.DiffResult)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyStatistic>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DB_Account_DailyStatistic>()
                .Property(e => e.Frozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<DeductMoney>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastConfig>()
                .Property(e => e.UserCost)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastConfig>()
                .Property(e => e.UserCash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.PayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.UserRate)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.AgentRate)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.SysRate)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.SysCash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.UserCash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.AgentPayGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.HFGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.SameGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastOrder>()
                .Property(e => e.IsProfit)
                .IsFixedLength();

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.Cost)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.BankCost)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.BankCost2)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.Cash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.SNum)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.ENum)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.SNum2)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.ENum2)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.BankSNum)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.BankENum)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.BankCost3)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.MinCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.MaxCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.MinCost2)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.MaxCost2)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.MinCost3)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.MaxCost3)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.InCost)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.InCost2)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.InCost3)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.Cost2)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastPayWay>()
                .Property(e => e.Cost3)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastSplit>()
                .Property(e => e.Profit)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastUserPay>()
                .Property(e => e.UserCost)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastUserPay>()
                .Property(e => e.UserCash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FastUserPay>()
                .Property(e => e.UserCost2)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FastUserPay>()
                .Property(e => e.UserCost3)
                .HasPrecision(6, 5);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.TotalAmoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.TotlaPoundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney2)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage2)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney2_0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage2_0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage2_1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney3)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage3)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney4)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage4)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney5)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage5)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney5_0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage5_0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney6)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage6)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney7)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage7)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney8)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage8)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney9)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage9)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Amoney10)
                .HasPrecision(19, 4);

            modelBuilder.Entity<FinTotal>()
                .Property(e => e.Poundage10)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobItem>()
                .Property(e => e.RunMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobItem>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobItem>()
                .Property(e => e.AgentGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobItem>()
                .Property(e => e.HFGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobItem>()
                .Property(e => e.RunGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobLog>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.TotalMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.HFGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.AgentGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.PayPoundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.CashPoundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.PayGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.CashGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.PayRate)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.PayMin)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.PayMax)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.CashRate)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.CashMin)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.CashMax)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.UPayRate)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.UPayMin)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.UPayMax)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.UCashRate)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.UCashMin)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.UCashMax)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobOrders>()
                .Property(e => e.SameGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobPayWay>()
                .Property(e => e.Cost)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobPayWay>()
                .Property(e => e.MinCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobPayWay>()
                .Property(e => e.MaxCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobPayWay>()
                .Property(e => e.CostAgent)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobPayWay>()
                .Property(e => e.MinAgentCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobPayWay>()
                .Property(e => e.MaxAgentCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobPayWay>()
                .Property(e => e.SNum)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobPayWay>()
                .Property(e => e.ENum)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.Cost)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.VIPCost)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.Cash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.VIPCash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.MinMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.MaxMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.Floated)
                .HasPrecision(3, 2);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.MinFloated)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.AdvFloated)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.MaxFloated)
                .HasPrecision(6, 5);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.DayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderCash>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderCash>()
                .Property(e => e.CashRate)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderCash>()
                .Property(e => e.Cash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderCash>()
                .Property(e => e.ECash)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderF2F>()
                .Property(e => e.PayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderF2F>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderF2F>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderHouse>()
                .Property(e => e.MonthRent)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderHouse>()
                .Property(e => e.SecurityMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderHouse>()
                .Property(e => e.PayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderHouse>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderHouse>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderHouse>()
                .Property(e => e.CashRate)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderProfitLog>()
                .Property(e => e.Profit)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderProfitLog>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderRecharge>()
                .Property(e => e.PayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderRecharge>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderRecharge>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Orders>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Orders>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Orders>()
                .Property(e => e.AgentPayGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrdersRec>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrdersRec>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrdersRefund>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrdersRepair>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderTransfer>()
                .Property(e => e.PayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderTransfer>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<OrderTransfer>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigChange>()
                .Property(e => e.Cash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigChange>()
                .Property(e => e.ECash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigChange>()
                .Property(e => e.Cash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigChange>()
                .Property(e => e.ECash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigChange>()
                .Property(e => e.APrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigChange>()
                .Property(e => e.BPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigChange>()
                .Property(e => e.CPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigOrder>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayConfigOrder>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<PayLog>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SalesLog>()
                .Property(e => e.AgentFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ShareTotal>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ShareTotal>()
                .Property(e => e.Profit)
                .HasPrecision(19, 4);

            modelBuilder.Entity<StopPayAudit>()
                .Property(e => e.Pic)
                .IsUnicode(false);

            modelBuilder.Entity<StopPayAudit>()
                .Property(e => e.StopPayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysAgent>()
                .Property(e => e.AgentFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysAgent>()
                .Property(e => e.ECash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysAgent>()
                .Property(e => e.Cash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysAgent>()
                .Property(e => e.ECash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysAgent>()
                .Property(e => e.Cash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysLog>()
                .Property(e => e.ControllerName)
                .IsUnicode(false);

            modelBuilder.Entity<SysLog>()
                .Property(e => e.ActionName)
                .IsUnicode(false);

            modelBuilder.Entity<SysLog>()
                .Property(e => e.RQData)
                .IsUnicode(false);

            modelBuilder.Entity<SysLog>()
                .Property(e => e.POSTData)
                .IsUnicode(false);

            modelBuilder.Entity<SysLog>()
                .Property(e => e.IP)
                .IsUnicode(false);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitA1)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitA2)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitA3)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitA4)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitA5)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitA6)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitU0)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitU1)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.PaySplitU2)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitA1)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitA2)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitA3)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitA4)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitA5)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitA6)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitU0)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitU1)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.VipSplitU2)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentPrice1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentPrice2)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentPrice3)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentPrice4)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentPrice5)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentPrice6)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentSplit0)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentSplit1)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentSplit2)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentSplit3)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentSplit4)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.AgentSplit5)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.SameAgent)
                .HasPrecision(5, 4);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitA1)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitA2)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitA3)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitA4)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitA5)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitA6)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitU0)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitU1)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysMoneySet>()
                .Property(e => e.JobSplitU2)
                .HasPrecision(7, 6);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.Cash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.ECash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.QCash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.Cash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.ECash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.CashMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.CtrlMoney1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.Level1Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.Level2Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.Level3Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.Level4Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.Level5Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.AgentGet)
                .HasPrecision(18, 8);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.PayConfigAgent)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.AutoCashMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.SysECash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.SysCash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.SysECash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.SysCash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.AgentECash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.AgentCash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.AgentECash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.AgentCash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.AuthPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SysSet>()
                .Property(e => e.EveryDayMaxCash)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaskTimeSet>()
                .Property(e => e.AllMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TaskTimeSet>()
                .Property(e => e.UsedMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TurnLog>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TurnProc>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserAuth>()
                .Property(e => e.AuthPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserFrozenLog>()
                .Property(e => e.StopPayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserLog>()
                .Property(e => e.BeforAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserLog>()
                .Property(e => e.BeforFrozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserLog>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserLog>()
                .Property(e => e.AfterAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserLog>()
                .Property(e => e.AfterFrozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayChange>()
                .Property(e => e.Cash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayChange>()
                .Property(e => e.ECash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayChange>()
                .Property(e => e.CashNew0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayChange>()
                .Property(e => e.ECashNew0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayChange>()
                .Property(e => e.Cash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayChange>()
                .Property(e => e.ECash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayChange>()
                .Property(e => e.CashNew1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayChange>()
                .Property(e => e.ECashNew1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPayCredit>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UserPromoteGet>()
                .Property(e => e.PromoteGet)
                .HasPrecision(18, 6);

            modelBuilder.Entity<Users>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.Frozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.Cash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.ECash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.Cash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.ECash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.StopPayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.CashFastPer)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.CashFastMax)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.TodayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.AutoCashMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.YAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.AllRec)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.LastRec)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Users>()
                .Property(e => e.Sp_Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.Frozen)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.Cash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.ECash0)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.Cash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.ECash1)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.StopPayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.CashFastPer)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.CashFastMax)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.TodayMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.AutoCashMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.YAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.AllRec)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.LastRec)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.Sp_Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.Operator)
                .IsUnicode(false);

            modelBuilder.Entity<UsersAmountErrLog>()
                .Property(e => e.typed)
                .IsUnicode(false);

            modelBuilder.Entity<VIPOrder>()
                .Property(e => e.Amoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<VIPOrder>()
                .Property(e => e.SplitMoney)
                .HasPrecision(19, 4);

            modelBuilder.Entity<VIPOrder>()
                .Property(e => e.Poundage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<VIPOrder>()
                .Property(e => e.HFGet)
                .HasPrecision(19, 4);

            modelBuilder.Entity<VIPOrder>()
                .Property(e => e.SameGet)
                .HasPrecision(19, 4);
            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S1_4)
                .HasPrecision(18, 4);

            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S1_5)
                .HasPrecision(18, 4);

            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S1_6)
                .HasPrecision(18, 4);

            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S1_1)
                .HasPrecision(18, 4);

            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S0_0)
                .HasPrecision(18, 4);

            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S2_6_5)
                .HasPrecision(18, 4);

            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S2_6_4)
                .HasPrecision(18, 4);

            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S2_5_4)
                .HasPrecision(18, 4);

            modelBuilder.Entity<BusinessShareProfit>()
                .Property(e => e.S3_6_5_4)
                .HasPrecision(18, 4);
        }
    }
}
