namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserBlackList")]
    public partial class UserBlackList
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CardNumber { get; set; }

        public byte State { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public DateTime AddTime { get; set; }

        public int AId { get; set; }

        public byte IsDel { get; set; }
    }
}
