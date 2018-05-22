namespace Test
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CTContext : DbContext
    {
        public CTContext()
            : base("name=CTContext")
        {
        }

        public virtual DbSet<BusinessInfo> BusinessInfo { get; set; }
        public virtual DbSet<BusinessUsers> BusinessUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
