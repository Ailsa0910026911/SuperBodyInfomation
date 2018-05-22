namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysAdmin")]
    public partial class SysAdmin
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(50)]
        public string PassWord { get; set; }

        [StringLength(50)]
        public string TrueName { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        public int AgentId { get; set; }

        public byte State { get; set; }

        public int LoginTimes { get; set; }

        public DateTime? LastTime { get; set; }

        [StringLength(50)]
        public string LoginIp { get; set; }

        public DateTime AddTime { get; set; }

        [StringLength(2000)]
        public string PowerID { get; set; }

        public byte IsDel { get; set; }

        public double PayGet { get; set; }

        [StringLength(20)]
        public string QQNum { get; set; }

        [StringLength(20)]
        public string QQName { get; set; }

        public byte QQState { get; set; }
    }
}
