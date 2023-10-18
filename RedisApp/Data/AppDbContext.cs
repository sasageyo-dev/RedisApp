using Microsoft.EntityFrameworkCore;
using RedisApp.Models;

namespace RedisApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
            
        }
        public DbSet<Driver> Drivers { get; set; }  
    }
}
