namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UsersPayCard")]
    public partial class UsersPayCard
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public short Type { get; set; }

        [StringLength(20)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Bank { get; set; }

        [StringLength(20)]
        public string Card { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
