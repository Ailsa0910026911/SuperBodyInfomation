//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class AdInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
        public string Url { get; set; }
        public Nullable<int> TId { get; set; }
        public string Tag { get; set; }
        public byte ModuleType { get; set; }
        public Nullable<byte> State { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public int Sort { get; set; }
        public byte IsDel { get; set; }
        public int AgentId { get; set; }
    }
}
