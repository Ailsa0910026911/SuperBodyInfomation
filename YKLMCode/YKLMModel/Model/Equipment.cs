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
    
    public partial class Equipment
    {
        public int Id { get; set; }
        public string No { get; set; }
        public string Keys { get; set; }
        public string IP { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte IsDel { get; set; }
        public string IMEI { get; set; }
        public Nullable<int> RqTimes { get; set; }
        public string RqType { get; set; }
        public string Mobile { get; set; }
        public string IfYY { get; set; }
        public string MobiType { get; set; }
        public string SysVer { get; set; }
        public string SoftVer { get; set; }
        public string SignalType { get; set; }
    }
}
