namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BasicMobile")]
    public partial class BasicMobile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Mobile { get; set; }

        [StringLength(20)]
        public string Province { get; set; }

        [StringLength(20)]
        public string City { get; set; }

        [StringLength(20)]
        public string Corp { get; set; }

        [StringLength(4)]
        public string AreaCode { get; set; }

        [StringLength(6)]
        public string PostCode { get; set; }

        public byte IsDel { get; set; }
    }
}
