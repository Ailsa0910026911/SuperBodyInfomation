namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserPic")]
    public partial class UserPic
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(20)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Pic { get; set; }

        public int? Sort { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
