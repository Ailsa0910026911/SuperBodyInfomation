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
    
    public partial class UsersPayCard
    {
        public int Id { get; set; }
        public int UId { get; set; }
        public short Type { get; set; }
        public string Name { get; set; }
        public string Bank { get; set; }
        public string Card { get; set; }
        public string Mobile { get; set; }
        public byte State { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte IsDel { get; set; }
    }
}
