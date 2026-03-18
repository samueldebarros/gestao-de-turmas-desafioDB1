
using Common.Domains;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Mappings;

public class EnturmamentoMapping : IEntityTypeConfiguration<Enturmamento>
{
    public void Configure(EntityTypeBuilder<Enturmamento> builder)
    {
        builder.ToTable("Enturmamentos");

        builder.HasKey(e => new { e.AlunoId, e.TurmaId });

        builder.Property(e => e.DataEnturmamento)
            .IsRequired()
            .HasColumnType("DATE");

        builder.Property(e => e.Situacao)
            .IsRequired()
            .HasColumnType("TINYINT")
            .HasDefaultValue(SituacaoEnturmamentoEnum.Ativo);

        builder.HasOne(e => e.Aluno)
            .WithMany(a => a.Enturmamentos)
            .HasForeignKey(e => e.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
