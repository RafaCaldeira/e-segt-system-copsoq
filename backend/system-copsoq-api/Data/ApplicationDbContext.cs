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

        public DbSet<Disparo> Disparos { get; set; }
        public DbSet<RespostaFuncionario> RespostasFuncionarios { get; set; }

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

            // --- Relações do Disparo ---

            modelBuilder.Entity<Funcionario>()
                .HasMany(f => f.Disparos)
                .WithOne(d => d.Funcionario)
                .HasForeignKey(d => d.FuncionarioID)
                .OnDelete(DeleteBehavior.Cascade); // Se Funcionario apagado, apaga Disparos

            modelBuilder.Entity<Questionario>()
                .HasMany<Disparo>() // Questionario tem muitos Disparos (Precisa de ICollection<Disparo> em Questionario.cs)
                .WithOne(d => d.Questionario) // Disparo tem um Questionario
                .HasForeignKey(d => d.QuestionarioID)
                .OnDelete(DeleteBehavior.Restrict);

                // --- Relações da RespostaFuncionario ---

            // Disparo <-> RespostaFuncionario (One-to-Many)
            modelBuilder.Entity<Disparo>()
                .HasMany(d => d.Respostas)
                .WithOne(r => r.Disparo)
                .HasForeignKey(r => r.DisparoID)
                .OnDelete(DeleteBehavior.Cascade); // Se Disparo apagado, apaga Respostas

            // Pergunta <-> RespostaFuncionario (One-to-Many)
            modelBuilder.Entity<Pergunta>()
                .HasMany(p => p.Respostas)
                .WithOne(r => r.Pergunta)
                .HasForeignKey(r => r.PerguntaID)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Índice para o token de acesso único ---
             modelBuilder.Entity<Disparo>()
                .HasIndex(d => d.TokenAcesso)
                .IsUnique();
        }

    }
}
