//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class ApplyCreditCard
    {
        public int Id { get; set; }
        public string OrderNum { get; set; }
        public int Uid { get; set; }
        public string UserName { get; set; }
        public string UserMobile { get; set; }
        public string UserIdCard { get; set; }
        public int BankId { get; set; }
        public int FirstAgentId { get; set; }
        public int AgentId { get; set; }
        public string Relation { get; set; }
        public int SettlementState { get; set; }
        public Nullable<decimal> SettlementAmount { get; set; }
        public Nullable<decimal> FirstAgentAmount { get; set; }
        public int FirstAgentAmountState { get; set; }
        public int State { get; set; }
        public System.DateTime AddTime { get; set; }
        public Nullable<System.DateTime> SettlementTime { get; set; }
        public Nullable<System.DateTime> FirstAgentTime { get; set; }
    }
}
