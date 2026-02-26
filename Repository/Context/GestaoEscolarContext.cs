using Common.Domains;
using Microsoft.EntityFrameworkCore;

namespace Repository.Context;

public class GestaoEscolarContext : DbContext 
{
    public GestaoEscolarContext(DbContextOptions<GestaoEscolarContext> options) : base(options) { }

    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Docente> Docentes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GestaoEscolarContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
