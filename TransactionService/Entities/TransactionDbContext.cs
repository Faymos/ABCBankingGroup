using Microsoft.EntityFrameworkCore;
using System.Transactions;
using TransactionService.Model;

namespace TransactionService.Entities
{
    public class TransactionDbContext : DbContext
    {
        public TransactionDbContext() { }

        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options) { }

      
        public DbSet<Transactions> transactions { get; set; }
        public DbSet<CustomerAccount> customerAccount { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transactions>()
                .HasKey(c => c.CustomerAccountId);
        }
    }
}
