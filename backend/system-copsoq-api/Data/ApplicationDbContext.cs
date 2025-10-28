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
        public DbSet<User> Usuarios { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Usuarios)
                .WithOne(u => u.Empresa)
                .HasForeignKey(u => u.EmpresaID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Empresa>()
                .Property(e => e.SetorAtuacao)
                .HasConversion<string>();

            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Funcionarios) 
                .WithOne(f => f.Empresa)      
                .HasForeignKey(f => f.EmpresaID) 
                .OnDelete(DeleteBehavior.Cascade);
                
        }
    }
}
