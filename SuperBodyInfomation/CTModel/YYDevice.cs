namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("YYDevice")]
    public partial class YYDevice
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(20)]
        public string DevId { get; set; }

        [StringLength(20)]
        public string PageId { get; set; }

        [StringLength(50)]
        public string Comment { get; set; }

        [Required]
        [StringLength(50)]
        public string UUID { get; set; }

        [Required]
        [StringLength(20)]
        public string Major { get; set; }

        [Required]
        [StringLength(20)]
        public string Minor { get; set; }

        public DateTime AddTime { get; set; }

        public byte ActState { get; set; }

        public byte State { get; set; }

        [StringLength(40)]
        public string Poi_Id { get; set; }

        [StringLength(20)]
        public string Poi_Appid { get; set; }

        public byte IsDel { get; set; }
    }
}
