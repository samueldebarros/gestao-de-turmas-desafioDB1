using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Mappings;

public class DocenteConfiguration : IEntityTypeConfiguration<Docente>
{
    public void Configure(EntityTypeBuilder<Docente> builder)
    {
        builder.ToTable("Docentes");
        builder.HasKey(x => x.Id);

        builder.Property(d => d.Nome)
            .IsRequired()
            .HasColumnType("VARCHAR(100)")
            .HasMaxLength(100);

        builder.Property(d => d.Cpf)
            .IsRequired()
            .HasColumnType("CHAR(11)")
            .HasMaxLength(11);

        builder.Property(d => d.Email)
            .IsRequired(false)
            .HasColumnType("VARCHAR(150)")
            .HasMaxLength(150);

        builder.Property(d => d.DataNascimento)
            .IsRequired()
            .HasColumnType("DATE");

        builder.Property(d => d.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(d => d.Cpf)
            .IsUnique();

        builder.HasIndex(d => d.Email)
            .IsUnique();

        builder.HasOne(d => d.Disciplina)
            .WithMany(disc => disc.Docentes)
            .HasForeignKey(d => d.DisciplinaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
