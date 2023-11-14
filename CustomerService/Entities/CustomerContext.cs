using CustomerService.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Entities
{
    public class CustomerContext : DbContext
    {
        public CustomerContext() { }
        public CustomerContext(DbContextOptions<CustomerContext> options) : base(options) { }
        public DbSet<Customer> customer { get; set; }
        public DbSet<Admin> admin { get; set; }
        public DbSet<CustomerAccount> customerAccount { get; set; }

       
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<CustomerAccount>()
        //        .HasKey(c => c.CustomerId);
        //}
    }
}
