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
    
    public partial class OrderHouse
    {
        public int Id { get; set; }
        public int UId { get; set; }
        public string OId { get; set; }
        public string HouseOwner { get; set; }
        public string Bank { get; set; }
        public string CardNum { get; set; }
        public Nullable<int> Province { get; set; }
        public Nullable<int> City { get; set; }
        public Nullable<int> District { get; set; }
        public string Bin { get; set; }
        public string Deposit { get; set; }
        public string Mobile { get; set; }
        public decimal MonthRent { get; set; }
        public decimal SecurityMoney { get; set; }
        public int PayMonth { get; set; }
        public decimal PayMoney { get; set; }
        public decimal Poundage { get; set; }
        public decimal Amoney { get; set; }
        public double SysRate { get; set; }
        public double UserRate { get; set; }
        public decimal CashRate { get; set; }
        public double AgentPayGet { get; set; }
        public int Agent { get; set; }
        public int AId { get; set; }
        public int FId { get; set; }
        public byte OrderState { get; set; }
        public byte PayState { get; set; }
        public byte AgentState { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte FState { get; set; }
        public Nullable<System.DateTime> FTime { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<byte> TrunType { get; set; }
        public string Remark { get; set; }
        public Nullable<System.DateTime> PayTime { get; set; }
        public Nullable<byte> PayWay { get; set; }
        public double AIdPayGet { get; set; }
    }
}
