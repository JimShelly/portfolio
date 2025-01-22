using Microsoft.EntityFrameworkCore;
using server.Orders;

namespace server.Data
{
    public class BowContext : DbContext
    {
        public BowContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Material> Materials { get; set; }
    }
}