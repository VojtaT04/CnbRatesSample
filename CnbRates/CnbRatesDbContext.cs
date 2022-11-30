using CnbRates.Model;
using Microsoft.EntityFrameworkCore;

namespace CnbRates
{
    public class CnbRatesDbContext : DbContext
    {
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server = (localdb)\\mssqllocaldb; Database = ExchangeRatesORM;");
        }

    }
}
