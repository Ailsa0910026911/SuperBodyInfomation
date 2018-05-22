namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BusinessUsers
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(11)]
        public string Phone { get; set; }

        [StringLength(50)]
        public string IDNumber { get; set; }

        public int? Type { get; set; }
    }
}
