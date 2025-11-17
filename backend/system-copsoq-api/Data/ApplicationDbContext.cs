using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Models;
using system_copsoq_api.Models.Formularios;
using system_copsoq_api.Models.Disparo; // <-- 1. ADICIONAR ESTE USING

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
        public DbSet<OpcaoResposta> OpcoesResposta { get; set; }

        // 2. Agora 'Disparo' e 'RespostaFuncionario' são encontrados
        public DbSet<Disparo> Disparos { get; set; }
        public DbSet<RespostaFuncionario> RespostasFuncionarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ... (O resto do seu ficheiro OnModelCreating está correto)
            base.OnModelCreating(modelBuilder);

            // ... (Empresa <-> User) ...
            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Usuarios)
                .WithOne(u => u.Empresa)
                .HasForeignKey(u => u.EmpresaID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ... (Enums) ...
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Empresa>()
                .Property(e => e.SetorAtuacao)
                .HasConversion<string>();
            
            modelBuilder.Entity<QuestionarioSetorAplicavel>()
                .Property(s => s.Setor)
                .HasConversion<string>();

            // ... (Empresa <-> Funcionario) ...
            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Funcionarios)
                .WithOne(f => f.Empresa)
                .HasForeignKey(f => f.EmpresaID)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Relações dos Formulários ---
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

            modelBuilder.Entity<Questionario>()
                .HasMany(q => q.OpcoesResposta)
                .WithOne(o => o.Questionario)
                .HasForeignKey(o => o.QuestionarioID)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Relações do Disparo (Agora funcionam) ---
            modelBuilder.Entity<Funcionario>()
                .HasMany(f => f.Disparos)
                .WithOne(d => d.Funcionario)
                .HasForeignKey(d => d.FuncionarioID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Questionario>()
                .HasMany<Disparo>() 
                .WithOne(d => d.Questionario) 
                .HasForeignKey(d => d.QuestionarioID)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Disparo>()
                .HasMany(d => d.Respostas)
                .WithOne(r => r.Disparo)
                .HasForeignKey(r => r.DisparoID)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Pergunta>()
                .HasMany(p => p.Respostas)
                .WithOne(r => r.Pergunta)
                .HasForeignKey(r => r.PerguntaID)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Disparo>()
                .HasIndex(d => d.TokenAcesso)
                .IsUnique();
        }
    }
}