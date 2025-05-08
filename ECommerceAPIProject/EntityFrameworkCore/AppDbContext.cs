using ECommerceAPIProject.EntityFrameworkCore.Entities;
using ECommerceAPIProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
namespace ECommerceAPIProject.EntityFrameworkCore
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

            builder.Entity<Product>()
               .Property(p => p.ArabicName)
            .IsRequired();

            builder.Entity<Product>()
                .Property(p => p.EnglishName)
                .IsRequired();
        }
    }
}