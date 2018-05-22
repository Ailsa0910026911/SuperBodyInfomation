namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FastUser")]
    public partial class FastUser
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(20)]
        public string TrueName { get; set; }

        [StringLength(20)]
        public string CardId { get; set; }

        [StringLength(20)]
        public string Bank { get; set; }

        [StringLength(20)]
        public string Card { get; set; }

        [StringLength(20)]
        public string Bin { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
