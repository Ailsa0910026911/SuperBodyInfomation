namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserTrail")]
    public partial class UserTrail
    {
        public int Id { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string X { get; set; }

        [StringLength(20)]
        public string Y { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public int UId { get; set; }
    }
}
