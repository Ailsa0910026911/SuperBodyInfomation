namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UsersFace")]
    public partial class UsersFace
    {
        public int Id { get; set; }

        public int? UId { get; set; }

        public byte CType { get; set; }

        [StringLength(20)]
        public string TrueName { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        public byte CardStae { get; set; }

        [StringLength(200)]
        public string RegAddress { get; set; }

        public int Agent { get; set; }

        public int AId { get; set; }

        public byte IsDaiLi { get; set; }

        public byte MobileType { get; set; }

        public int Times { get; set; }

        [StringLength(2000)]
        public string When { get; set; }

        public string Remark { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public DateTime UpdateTime { get; set; }

        public byte IsNew { get; set; }
    }
}
