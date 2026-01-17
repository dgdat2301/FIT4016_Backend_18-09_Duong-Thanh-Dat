using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using OrderManagement.Models;

namespace OrderManagement.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext() : base("name=OrderManagementDB")
        {
            Database.SetInitializer<OrderDbContext>(new CreateDatabaseIfNotExists<OrderDbContext>());
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Configure Product entity
            modelBuilder.Entity<Product>()
                .Property(p => p.CreatedAt)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Product>()
                .Property(p => p.UpdatedAt)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed);

            // Configure Order entity
            modelBuilder.Entity<Order>()
                .Property(o => o.CreatedAt)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Order>()
                .Property(o => o.UpdatedAt)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed);

            // Configure relationships
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Product)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.ProductId)
                .WillCascadeOnDelete(false);

            // Configure decimal precision for Price
            var decimalPropertyConfiguration = modelBuilder.Entity<Product>()
                .Property(p => p.Price);

            decimalPropertyConfiguration.HasColumnType("decimal").HasPrecision(18, 2);
        }
    }
}