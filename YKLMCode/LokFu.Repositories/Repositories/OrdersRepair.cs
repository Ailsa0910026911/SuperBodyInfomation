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
    
    public partial class OrdersRepair
    {
        public int Id { get; set; }
        public decimal Amoney { get; set; }
        public byte TState { get; set; }
        public string TNum { get; set; }
        public string Remark { get; set; }
        public string Pic { get; set; }
        public string CreateAdminName { get; set; }
        public int CreateAdminId { get; set; }
        public string AuditAdminName { get; set; }
        public Nullable<int> AuditAdminId { get; set; }
        public Nullable<System.DateTime> AuditTime { get; set; }
        public string AuditRemark { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte RepairType { get; set; }
    }
}
