using Microsoft.EntityFrameworkCore;
using platterr_api.Entities;

namespace platterr_api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Platter> Platters { get; set; }

        public DbSet<Order> Orders { get; set; }
    }
}