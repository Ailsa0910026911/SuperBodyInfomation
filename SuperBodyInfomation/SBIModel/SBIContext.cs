namespace SBIModel
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class SBIContext : DbContext
    {
        public SBIContext()
            : base("name=SBIContext")
        {
            Database.SetInitializer<SBIContext>(new DropCreateDatabaseAlways<SBIContext>());
        }

        public virtual DbSet<ordersinfo> ordersinfo { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ordersinfo>()
                .Property(e => e.ID)
                .IsUnicode(false);

            modelBuilder.Entity<ordersinfo>()
                .Property(e => e.Phone)
                .IsUnicode(false);

            modelBuilder.Entity<ordersinfo>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<ordersinfo>()
                .Property(e => e.Adress)
                .IsUnicode(false);

            modelBuilder.Entity<ordersinfo>()
                .Property(e => e.OExtension)
                .IsUnicode(false);

            modelBuilder.Entity<ordersinfo>()
                .Property(e => e.Remark)
                .IsUnicode(false);
        }
    }
}
