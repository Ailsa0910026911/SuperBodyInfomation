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
    
    public partial class VIPOrder
    {
        public int Id { get; set; }
        public string VName { get; set; }
        public int UId { get; set; }
        public string TNum { get; set; }
        public decimal Amoney { get; set; }
        public decimal SplitMoney { get; set; }
        public byte SplitState { get; set; }
        public decimal Poundage { get; set; }
        public decimal HFGet { get; set; }
        public int PayWay { get; set; }
        public byte State { get; set; }
        public byte PayState { get; set; }
        public Nullable<System.DateTime> PayTime { get; set; }
        public System.DateTime AddTime { get; set; }
        public string Remark { get; set; }
        public int Days { get; set; }
        public byte UserState { get; set; }
        public Nullable<System.DateTime> UserTime { get; set; }
        public int Agent { get; set; }
        public decimal SameGet { get; set; }
    }
}
