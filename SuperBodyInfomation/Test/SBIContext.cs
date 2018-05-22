namespace Test
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
        }

        public virtual DbSet<goods> goods { get; set; }
        public virtual DbSet<ordersinfo> ordersinfo { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<goods>()
                .Property(e => e.ID)
                .IsUnicode(false);

            modelBuilder.Entity<goods>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<goods>()
                .Property(e => e.Describe)
                .IsUnicode(false);

            modelBuilder.Entity<goods>()
                .Property(e => e.Img)
                .IsUnicode(false);

            modelBuilder.Entity<goods>()
                .Property(e => e.Extension)
                .IsUnicode(false);

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
