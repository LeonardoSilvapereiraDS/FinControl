using FinControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinControl.Infrastructure.Persistence;

public sealed class FinControlDbContext : DbContext
{
    public FinControlDbContext(DbContextOptions<FinControlDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Conta> Contas => Set<Conta>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Transacao> Transacoes => Set<Transacao>();

    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();

    public DbSet<MetaFinanceira> MetasFinanceiras => Set<MetaFinanceira>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinControlDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
