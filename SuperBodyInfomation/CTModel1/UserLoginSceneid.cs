namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserLoginSceneid")]
    public partial class UserLoginSceneid
    {
        public int Id { get; set; }

        public int? UId { get; set; }

        [StringLength(50)]
        public string Token { get; set; }

        public int Times { get; set; }

        [Required]
        [StringLength(50)]
        public string Sceneid { get; set; }

        [StringLength(200)]
        public string Pic { get; set; }

        public DateTime? LoginTime { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
