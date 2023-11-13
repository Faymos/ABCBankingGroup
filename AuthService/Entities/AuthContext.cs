using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Entities
{
    public class AuthContext : DbContext
    {
        public AuthContext() { }
        public AuthContext(DbContextOptions<AuthContext> options): base(options) { }
        public DbSet<User> user { get; set; }
        public DbSet<Admin> admin { get; set; }
    }
}
