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
    
    public partial class SysAdmin
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string TrueName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public int AgentId { get; set; }
        public byte State { get; set; }
        public int LoginTimes { get; set; }
        public Nullable<System.DateTime> LastTime { get; set; }
        public string LoginIp { get; set; }
        public System.DateTime AddTime { get; set; }
        public string PowerID { get; set; }
        public byte IsDel { get; set; }
        public double PayGet { get; set; }
        public string QQNum { get; set; }
        public string QQName { get; set; }
        public byte QQState { get; set; }
    }
}