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
    
    public partial class ApplyJoin
    {
        public int Id { get; set; }
        public byte ServiceType { get; set; }
        public byte ApplyType { get; set; }
        public string Linker { get; set; }
        public string Mobile { get; set; }
        public string ComName { get; set; }
        public Nullable<int> Province { get; set; }
        public Nullable<int> City { get; set; }
        public Nullable<int> District { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte State { get; set; }
        public int AgentId { get; set; }
        public int AgentAId { get; set; }
        public string Remark { get; set; }
        public Nullable<int> TiePaiAgentId { get; set; }
        public string TiePaiAgentName { get; set; }
        public string AgentName { get; set; }
    }
}