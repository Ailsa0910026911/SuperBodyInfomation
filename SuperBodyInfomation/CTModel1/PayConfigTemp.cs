namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PayConfigTemp")]
    public partial class PayConfigTemp
    {
        public int Id { get; set; }

        public int? PCCId { get; set; }

        public int PId { get; set; }

        public double Cost { get; set; }

        public byte IsDel { get; set; }

        public byte State { get; set; }
    }
}
