using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Mappings;

internal class AlunoConfiguration : IEntityTypeConfiguration<Aluno>
{
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.ToTable("Alunos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Matricula)
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnType("VARCHAR(20)")
            .HasDefaultValueSql("'000000'");

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasColumnType("VARCHAR(100)")
            .HasMaxLength(100);

        builder.Property(x => x.Cpf)
            .IsRequired()
            .HasMaxLength(14)
            .HasColumnType("CHAR(14)");
        
        builder.Property(x => x.DataNascimento)
            .IsRequired()
            .HasColumnType("date")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.Sexo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnType("VARCHAR(20)")
            .HasDefaultValueSql("'NaoInformado'");

        builder.Property(x => x.Email)
            .HasMaxLength(150)
            .IsRequired(false)
            .HasColumnType("VARCHAR(150)");

        builder.Property(x => x.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Cpf)
            .IsUnique();

        builder.HasIndex(x => x.Matricula)
            .IsUnique();

    }
}
