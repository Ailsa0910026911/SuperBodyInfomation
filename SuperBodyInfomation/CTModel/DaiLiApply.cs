namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DaiLiApply")]
    public partial class DaiLiApply
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TureName { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public int Agent { get; set; }

        public int AId { get; set; }

        public byte OrderState { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        [StringLength(20)]
        public string Area { get; set; }

        [StringLength(20)]
        public string IdCard { get; set; }

        [StringLength(20)]
        public string IdCardPlace { get; set; }

        [StringLength(200)]
        public string IdCardPic { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(50)]
        public string YingYeName { get; set; }

        [StringLength(20)]
        public string YingYeNum { get; set; }

        [StringLength(200)]
        public string YingYePic { get; set; }

        [StringLength(20)]
        public string SuiWuNum { get; set; }

        [StringLength(200)]
        public string SuiWuPic { get; set; }

        [StringLength(20)]
        public string ZuZhiNum { get; set; }

        [StringLength(200)]
        public string ZuZhiPic { get; set; }

        [StringLength(200)]
        public string BankPic { get; set; }

        [StringLength(200)]
        public string Photo { get; set; }

        [StringLength(200)]
        public string Pic1 { get; set; }

        [StringLength(200)]
        public string Pic2 { get; set; }

        [StringLength(200)]
        public string Pic3 { get; set; }

        [StringLength(200)]
        public string Pic4 { get; set; }
    }
}
