﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace YKLMModel.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class YiKaLianMengEntities : DbContext
    {
        public YiKaLianMengEntities()
            : base("name=YiKaLianMengEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
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
    }
}
