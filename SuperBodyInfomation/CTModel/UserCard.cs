namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserCard")]
    public partial class UserCard
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public short Type { get; set; }

        [StringLength(20)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Bank { get; set; }

        [StringLength(20)]
        public string Card { get; set; }

        public int? Province { get; set; }

        public int? City { get; set; }

        public int? District { get; set; }

        [StringLength(20)]
        public string Bin { get; set; }

        [Required]
        [StringLength(50)]
        public string Deposit { get; set; }

        public byte IsDel { get; set; }

        public int? BId { get; set; }

        [Required]
        [StringLength(20)]
        public string Mobile { get; set; }

        [Required]
        [StringLength(200)]
        public string Pic { get; set; }

        [Required]
        [StringLength(20)]
        public string ScanNo { get; set; }

        public byte State { get; set; }

        [StringLength(10)]
        public string CVV { get; set; }

        public byte? BillDay { get; set; }

        public byte? RefundDay { get; set; }

        [StringLength(5)]
        public string ValidYear { get; set; }

        [StringLength(5)]
        public string ValidMonth { get; set; }

        public DateTime? UnBindingTime { get; set; }

        public DateTime? AddTime { get; set; }
    }
}
