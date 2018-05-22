namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("JobItem")]
    public partial class JobItem
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string TNum { get; set; }

        [StringLength(50)]
        public string RunNum { get; set; }

        [Column(TypeName = "money")]
        public decimal RunMoney { get; set; }

        public DateTime RunTime { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentGet { get; set; }

        [Column(TypeName = "money")]
        public decimal HFGet { get; set; }

        [Column(TypeName = "money")]
        public decimal RunGet { get; set; }

        public byte State { get; set; }

        public DateTime? AddTime { get; set; }

        public byte RunType { get; set; }

        public byte RunState { get; set; }

        public DateTime? RunedTime { get; set; }

        public int PayWay { get; set; }

        public byte IsDel { get; set; }

        public int UserCardId { get; set; }

        [StringLength(200)]
        public string Remark { get; set; }

        public int RunSort { get; set; }
    }
}
