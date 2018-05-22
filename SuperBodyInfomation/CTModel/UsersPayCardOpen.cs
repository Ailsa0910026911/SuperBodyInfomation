namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UsersPayCardOpen")]
    public partial class UsersPayCardOpen
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(30)]
        public string CardNum { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        [Required]
        [StringLength(100)]
        public string Token { get; set; }

        public DateTime STime { get; set; }

        public DateTime ETime { get; set; }

        public int PayWay { get; set; }

        public byte State { get; set; }

        public string RqData { get; set; }

        public byte IsDel { get; set; }
    }
}
