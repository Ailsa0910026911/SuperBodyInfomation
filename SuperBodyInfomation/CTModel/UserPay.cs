namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserPay")]
    public partial class UserPay
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public int PId { get; set; }

        public double Cost { get; set; }

        public byte IsDel { get; set; }
    }
}
