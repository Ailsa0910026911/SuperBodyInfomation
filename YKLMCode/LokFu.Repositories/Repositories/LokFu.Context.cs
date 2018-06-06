﻿//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace LokFu.Repositories.Repositories
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class LokFuEntity : DbContext
    {
        public LokFuEntity()
            : base("name=LokFuEntity")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<AdInfo> AdInfo { get; set; }
        public DbSet<AdTag> AdTag { get; set; }
        public DbSet<AgentType> AgentType { get; set; }
        public DbSet<APPBlock> APPBlock { get; set; }
        public DbSet<ApplyCredit> ApplyCredit { get; set; }
        public DbSet<ApplyCreditCard> ApplyCreditCard { get; set; }
        public DbSet<ApplyJoin> ApplyJoin { get; set; }
        public DbSet<ApplyLoan> ApplyLoan { get; set; }
        public DbSet<APPModule> APPModule { get; set; }
        public DbSet<AppUpdate> AppUpdate { get; set; }
        public DbSet<Attach> Attach { get; set; }
        public DbSet<BanKaList> BanKaList { get; set; }
        public DbSet<BanKaOrder> BanKaOrder { get; set; }
        public DbSet<BanKaType> BanKaType { get; set; }
        public DbSet<BaoConfig> BaoConfig { get; set; }
        public DbSet<BaoLog> BaoLog { get; set; }
        public DbSet<BaoStory> BaoStory { get; set; }
        public DbSet<BaoUsers> BaoUsers { get; set; }
        public DbSet<BasicBank> BasicBank { get; set; }
        public DbSet<BasicBankCard> BasicBankCard { get; set; }
        public DbSet<BasicBankInfo> BasicBankInfo { get; set; }
        public DbSet<BasicCarBrand> BasicCarBrand { get; set; }
        public DbSet<BasicCardBin> BasicCardBin { get; set; }
        public DbSet<BasicCity> BasicCity { get; set; }
        public DbSet<BasicCode> BasicCode { get; set; }
        public DbSet<BasicDesc> BasicDesc { get; set; }
        public DbSet<BasicDistrict> BasicDistrict { get; set; }
        public DbSet<BasicMobile> BasicMobile { get; set; }
        public DbSet<BasicProvince> BasicProvince { get; set; }
        public DbSet<Card> Card { get; set; }
        public DbSet<CheckResult> CheckResult { get; set; }
        public DbSet<CheckTask> CheckTask { get; set; }
        public DbSet<CheckUser> CheckUser { get; set; }
        public DbSet<CheckUserMoney> CheckUserMoney { get; set; }
        public DbSet<CutAct> CutAct { get; set; }
        public DbSet<CutLog> CutLog { get; set; }
        public DbSet<CutUsers> CutUsers { get; set; }
        public DbSet<DaiLiApply> DaiLiApply { get; set; }
        public DbSet<DaiLiOrder> DaiLiOrder { get; set; }
        public DbSet<DB_Account_DailyCompared> DB_Account_DailyCompared { get; set; }
        public DbSet<DB_Account_DailyStatistic> DB_Account_DailyStatistic { get; set; }
        public DbSet<DeductMoney> DeductMoney { get; set; }
        public DbSet<DownFile> DownFile { get; set; }
        public DbSet<DownFileTag> DownFileTag { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<FastConfig> FastConfig { get; set; }
        public DbSet<FastOrder> FastOrder { get; set; }
        public DbSet<FastOrderChange> FastOrderChange { get; set; }
        public DbSet<FastOrderOP> FastOrderOP { get; set; }
        public DbSet<FastPayWay> FastPayWay { get; set; }
        public DbSet<FastSplit> FastSplit { get; set; }
        public DbSet<FastUser> FastUser { get; set; }
        public DbSet<FastUserPay> FastUserPay { get; set; }
        public DbSet<FinTotal> FinTotal { get; set; }
        public DbSet<HaoWool> HaoWool { get; set; }
        public DbSet<JobItem> JobItem { get; set; }
        public DbSet<JobLog> JobLog { get; set; }
        public DbSet<JobOrders> JobOrders { get; set; }
        public DbSet<JobPayWay> JobPayWay { get; set; }
        public DbSet<JobSet> JobSet { get; set; }
        public DbSet<JobUserPay> JobUserPay { get; set; }
        public DbSet<MsgAbout> MsgAbout { get; set; }
        public DbSet<MsgAdmin> MsgAdmin { get; set; }
        public DbSet<MsgCallBack> MsgCallBack { get; set; }
        public DbSet<MsgHelp> MsgHelp { get; set; }
        public DbSet<MsgNotice> MsgNotice { get; set; }
        public DbSet<MsgTemplate> MsgTemplate { get; set; }
        public DbSet<MsgUser> MsgUser { get; set; }
        public DbSet<OrderCash> OrderCash { get; set; }
        public DbSet<OrderCashLog> OrderCashLog { get; set; }
        public DbSet<OrderF2F> OrderF2F { get; set; }
        public DbSet<OrderHouse> OrderHouse { get; set; }
        public DbSet<OrderProfitLog> OrderProfitLog { get; set; }
        public DbSet<OrderRecharge> OrderRecharge { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrdersDDLog> OrdersDDLog { get; set; }
        public DbSet<OrdersPayOnly> OrdersPayOnly { get; set; }
        public DbSet<OrdersRec> OrdersRec { get; set; }
        public DbSet<OrdersRefund> OrdersRefund { get; set; }
        public DbSet<OrdersRefundLog> OrdersRefundLog { get; set; }
        public DbSet<OrdersRepair> OrdersRepair { get; set; }
        public DbSet<OrdersRepairLog> OrdersRepairLog { get; set; }
        public DbSet<OrderTransfer> OrderTransfer { get; set; }
        public DbSet<PayConfig> PayConfig { get; set; }
        public DbSet<PayConfigChange> PayConfigChange { get; set; }
        public DbSet<PayConfigOrder> PayConfigOrder { get; set; }
        public DbSet<PayConfigTemp> PayConfigTemp { get; set; }
        public DbSet<PayLog> PayLog { get; set; }
        public DbSet<QRCode> QRCode { get; set; }
        public DbSet<SalesLog> SalesLog { get; set; }
        public DbSet<ShareTotal> ShareTotal { get; set; }
        public DbSet<SMSCode> SMSCode { get; set; }
        public DbSet<SMSLog> SMSLog { get; set; }
        public DbSet<StopPayAudit> StopPayAudit { get; set; }
        public DbSet<SysAdmin> SysAdmin { get; set; }
        public DbSet<SysAgent> SysAgent { get; set; }
        public DbSet<SysAgentSet> SysAgentSet { get; set; }
        public DbSet<SysAsk> SysAsk { get; set; }
        public DbSet<SysControl> SysControl { get; set; }
        public DbSet<sysdiagrams> sysdiagrams { get; set; }
        public DbSet<SysLog> SysLog { get; set; }
        public DbSet<SysMenu> SysMenu { get; set; }
        public DbSet<SysMoneySet> SysMoneySet { get; set; }
        public DbSet<SysPower> SysPower { get; set; }
        public DbSet<SysSet> SysSet { get; set; }
        public DbSet<TaskCash> TaskCash { get; set; }
        public DbSet<TaskCashInfo> TaskCashInfo { get; set; }
        public DbSet<TaskOrders> TaskOrders { get; set; }
        public DbSet<TaskTimeSet> TaskTimeSet { get; set; }
        public DbSet<TurnLog> TurnLog { get; set; }
        public DbSet<TurnProc> TurnProc { get; set; }
        public DbSet<Turntable> Turntable { get; set; }
        public DbSet<TurnUsers> TurnUsers { get; set; }
        public DbSet<UserAsk> UserAsk { get; set; }
        public DbSet<UserAuth> UserAuth { get; set; }
        public DbSet<UserBlackList> UserBlackList { get; set; }
        public DbSet<UserCard> UserCard { get; set; }
        public DbSet<UserCardOpen> UserCardOpen { get; set; }
        public DbSet<UserFrozenLog> UserFrozenLog { get; set; }
        public DbSet<UserLog> UserLog { get; set; }
        public DbSet<UserLoginSceneid> UserLoginSceneid { get; set; }
        public DbSet<UserMaillist> UserMaillist { get; set; }
        public DbSet<UserPay> UserPay { get; set; }
        public DbSet<UserPayAgent> UserPayAgent { get; set; }
        public DbSet<UserPayChange> UserPayChange { get; set; }
        public DbSet<UserPayCredit> UserPayCredit { get; set; }
        public DbSet<UserPayTemp> UserPayTemp { get; set; }
        public DbSet<UserPic> UserPic { get; set; }
        public DbSet<UserPromoteGet> UserPromoteGet { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UsersAmountErrLog> UsersAmountErrLog { get; set; }
        public DbSet<UsersFace> UsersFace { get; set; }
        public DbSet<UsersMoveLog> UsersMoveLog { get; set; }
        public DbSet<UsersPayCard> UsersPayCard { get; set; }
        public DbSet<UsersPayCardOpen> UsersPayCardOpen { get; set; }
        public DbSet<UserTrack> UserTrack { get; set; }
        public DbSet<UserTrail> UserTrail { get; set; }
        public DbSet<VIPOrder> VIPOrder { get; set; }
        public DbSet<WeiXinUsers> WeiXinUsers { get; set; }
        public DbSet<YYApply> YYApply { get; set; }
        public DbSet<YYDaily> YYDaily { get; set; }
        public DbSet<YYDevice> YYDevice { get; set; }
        public DbSet<YYPage> YYPage { get; set; }
    
        public virtual ObjectResult<string> SerializeJSON(string parameterSQL)
        {
            var parameterSQLParameter = parameterSQL != null ?
                new ObjectParameter("ParameterSQL", parameterSQL) :
                new ObjectParameter("ParameterSQL", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("SerializeJSON", parameterSQLParameter);
        }
    
        public virtual int sp_alterdiagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_alterdiagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_creatediagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_creatediagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_dropdiagram(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_dropdiagram", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagramdefinition_Result> sp_helpdiagramdefinition(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagramdefinition_Result>("sp_helpdiagramdefinition", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagrams_Result> sp_helpdiagrams(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagrams_Result>("sp_helpdiagrams", diagramnameParameter, owner_idParameter);
        }
    
        public virtual int sp_renamediagram(string diagramname, Nullable<int> owner_id, string new_diagramname)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var new_diagramnameParameter = new_diagramname != null ?
                new ObjectParameter("new_diagramname", new_diagramname) :
                new ObjectParameter("new_diagramname", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_renamediagram", diagramnameParameter, owner_idParameter, new_diagramnameParameter);
        }
    
        public virtual int sp_upgraddiagrams()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_upgraddiagrams");
        }
    
        public virtual int SP_UsersMoney(Nullable<int> uId, string oId, Nullable<decimal> amount, Nullable<short> oType, string remark, Nullable<short> subType, ObjectParameter result)
        {
            var uIdParameter = uId.HasValue ?
                new ObjectParameter("UId", uId) :
                new ObjectParameter("UId", typeof(int));
    
            var oIdParameter = oId != null ?
                new ObjectParameter("OId", oId) :
                new ObjectParameter("OId", typeof(string));
    
            var amountParameter = amount.HasValue ?
                new ObjectParameter("Amount", amount) :
                new ObjectParameter("Amount", typeof(decimal));
    
            var oTypeParameter = oType.HasValue ?
                new ObjectParameter("OType", oType) :
                new ObjectParameter("OType", typeof(short));
    
            var remarkParameter = remark != null ?
                new ObjectParameter("Remark", remark) :
                new ObjectParameter("Remark", typeof(string));
    
            var subTypeParameter = subType.HasValue ?
                new ObjectParameter("SubType", subType) :
                new ObjectParameter("SubType", typeof(short));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SP_UsersMoney", uIdParameter, oIdParameter, amountParameter, oTypeParameter, remarkParameter, subTypeParameter, result);
        }
    }
}