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
    
    public partial class TaskCashInfo
    {
        public int Id { get; set; }
        public string OId { get; set; }
        public Nullable<int> TId { get; set; }
        public byte State { get; set; }
        public byte OState { get; set; }
        public byte NState { get; set; }
        public System.DateTime AddTime { get; set; }
        public string Remark { get; set; }
        public Nullable<System.DateTime> STime { get; set; }
        public Nullable<System.DateTime> ETime { get; set; }
    }
}
