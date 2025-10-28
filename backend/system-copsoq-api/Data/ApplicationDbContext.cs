using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Models;
using system_copsoq_api.Models.Formularios;

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

        public DbSet<Questionario> Questionarios { get; set; }
        public DbSet<Dimensao> Dimensoes { get; set; }
        public DbSet<Pergunta> Perguntas { get; set; }
        public DbSet<QuestionarioSetorAplicavel> QuestionarioSetoresAplicaveis { get; set; }

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


            // Parte dos formulários:\\

            modelBuilder.Entity<Questionario>()
                .HasMany(q => q.Dimensoes)
                .WithOne(d => d.Questionario)
                .HasForeignKey(d => d.QuestionarioID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Questionario>()
                .HasMany(q => q.Perguntas)
                .WithOne(p => p.Questionario)
                .HasForeignKey(p => p.QuestionarioID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dimensao>()
                .HasMany(d => d.Perguntas)
                .WithOne(p => p.Dimensao)
                .HasForeignKey(p => p.DimensaoID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Questionario>()
                .HasMany(q => q.SetoresAplicaveis)
                .WithOne(s => s.Questionario)
                .HasForeignKey(s => s.QuestionarioID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pergunta>()
                .Property(p => p.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<QuestionarioSetorAplicavel>()
                .Property(s => s.Setor)
                .HasConversion<string>();
        }

    }
}
