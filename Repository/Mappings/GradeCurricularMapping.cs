

using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Mappings;

public class GradeCurricularMapping : IEntityTypeConfiguration<GradeCurricular>
{
    public void Configure(EntityTypeBuilder<GradeCurricular> builder)
    {
        builder.ToTable("GradeCurricular");

        builder.HasKey(g => new { g.TurmaId, g.DisciplinaId });

        builder.HasOne(g => g.Disciplina)
            .WithMany()
            .HasForeignKey(g => g.DisciplinaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Docente)
            .WithMany()
            .HasForeignKey(g => g.DocenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}