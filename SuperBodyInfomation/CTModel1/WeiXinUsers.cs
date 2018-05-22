namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class WeiXinUsers
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(100)]
        public string OpenId { get; set; }

        public int ComeId { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }

        [Required]
        [StringLength(100)]
        public string NickName { get; set; }

        [StringLength(50)]
        public string Sex { get; set; }

        [StringLength(100)]
        public string Province { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [StringLength(1000)]
        public string HeadImgUrl { get; set; }

        [StringLength(1000)]
        public string Privilege { get; set; }

        [StringLength(100)]
        public string Unionid { get; set; }
    }
}
