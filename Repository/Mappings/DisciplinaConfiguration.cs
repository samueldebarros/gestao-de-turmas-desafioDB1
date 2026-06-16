using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Mappings
{
    public class DisciplinaConfiguration : IEntityTypeConfiguration<Disciplina>
    {
        public void Configure(EntityTypeBuilder<Disciplina> builder)
        {
            builder.ToTable("Disciplinas", tb =>
            {
                tb.HasCheckConstraint("CK_Disciplinas_CargaHoraria", "[CargaHoraria] > 0");
            });
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Nome)
                .IsRequired()
                .HasColumnType("VARCHAR(100)")
                .HasMaxLength(100);

            builder.Property(d => d.CargaHoraria)
                .IsRequired();

            builder.Property(d => d.Ementa)
                .IsRequired(false)
                .HasColumnType("VARCHAR(500)")
                .HasMaxLength(500);

            builder.Property(d => d.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasIndex(d => d.Nome)
                .IsUnique();
        }
    }
}
