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
    
    public partial class JobItem
    {
        public int Id { get; set; }
        public int UId { get; set; }
        public string TNum { get; set; }
        public string RunNum { get; set; }
        public decimal RunMoney { get; set; }
        public System.DateTime RunTime { get; set; }
        public decimal Poundage { get; set; }
        public decimal AgentGet { get; set; }
        public decimal HFGet { get; set; }
        public decimal RunGet { get; set; }
        public byte State { get; set; }
        public Nullable<System.DateTime> AddTime { get; set; }
        public byte RunType { get; set; }
        public byte RunState { get; set; }
        public Nullable<System.DateTime> RunedTime { get; set; }
        public int PayWay { get; set; }
        public int UserCardId { get; set; }
        public string Remark { get; set; }
        public int RunSort { get; set; }
    }
}
