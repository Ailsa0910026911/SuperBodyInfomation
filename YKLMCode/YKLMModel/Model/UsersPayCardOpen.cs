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
    
    public partial class UsersPayCardOpen
    {
        public int Id { get; set; }
        public int UId { get; set; }
        public string CardNum { get; set; }
        public string Mobile { get; set; }
        public string Token { get; set; }
        public System.DateTime STime { get; set; }
        public System.DateTime ETime { get; set; }
        public int PayWay { get; set; }
        public byte State { get; set; }
        public string RqData { get; set; }
        public byte IsDel { get; set; }
    }
}
