namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserAuth")]
    public partial class UserAuth
    {
        public int Id { get; set; }

        public int? UId { get; set; }

        [StringLength(20)]
        public string OId { get; set; }

        [StringLength(30)]
        public string BankAccount { get; set; }

        [StringLength(20)]
        public string AccountName { get; set; }

        [StringLength(50)]
        public string IdentityCode { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        [StringLength(10)]
        public string CVV { get; set; }

        [StringLength(20)]
        public string EndDate { get; set; }

        [StringLength(20)]
        public string RetCode { get; set; }

        [StringLength(1000)]
        public string RetMsg { get; set; }

        public string RetLog { get; set; }

        public byte IsCharge { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public byte AuthType { get; set; }

        [Column(TypeName = "money")]
        public decimal AuthPrice { get; set; }
    }
}
