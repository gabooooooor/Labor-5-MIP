using System.Data.Entity;

namespace WindowsFormsApp6
{
    public class StoreDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public StoreDbContext() : base("name=StoreDb")
        {
        }
    }
}