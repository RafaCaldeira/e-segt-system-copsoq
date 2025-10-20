using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Models;

namespace system_copsoq_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Empresa> Empresas { get; set; }
    }
}
