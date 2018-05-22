namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserPayTemp")]
    public partial class UserPayTemp
    {
        public int Id { get; set; }

        public int? UPCId { get; set; }

        public int UId { get; set; }

        public int PId { get; set; }

        public double Cost { get; set; }

        public byte IsDel { get; set; }

        public double ACost { get; set; }
    }
}
