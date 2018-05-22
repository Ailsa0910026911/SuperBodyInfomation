namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserMaillist")]
    public partial class UserMaillist
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(50)]
        public string Mobile { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime UpTime { get; set; }

        public int State { get; set; }

        public byte IsDel { get; set; }

        [StringLength(200)]
        public string IMEI { get; set; }
    }
}
