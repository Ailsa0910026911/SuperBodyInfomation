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
    
    public partial class UsersMoveLog
    {
        public int Id { get; set; }
        public int FromSAId { get; set; }
        public string FromName { get; set; }
        public int ToSAId { get; set; }
        public string ToName { get; set; }
        public int UId { get; set; }
        public System.DateTime AddTime { get; set; }
        public string UTrueName { get; set; }
        public string OpName { get; set; }
        public byte Type { get; set; }
        public string Remark { get; set; }
        public string Tel { get; set; }
    }
}
