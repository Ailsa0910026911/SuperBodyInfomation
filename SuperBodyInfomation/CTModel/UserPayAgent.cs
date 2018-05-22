namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserPayAgent")]
    public partial class UserPayAgent
    {
        public int Id { get; set; }

        public int AId { get; set; }

        public int PId { get; set; }

        public double Cost { get; set; }

        public byte IsDel { get; set; }
    }
}
