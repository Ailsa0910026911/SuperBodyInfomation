namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BasicCode")]
    public partial class BasicCode
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CharCode { get; set; }

        [Required]
        [StringLength(50)]
        public string CharText { get; set; }

        public int Sort { get; set; }

        public byte IsDel { get; set; }
    }
}
