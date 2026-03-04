using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Mappings
{
    public class DisciplinaConfiguration : IEntityTypeConfiguration<Disciplina>
    {
        public void Configure(EntityTypeBuilder<Disciplina> builder)
        {
            builder.ToTable("Disciplinas");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Nome)
                .IsRequired()
                .HasColumnType("VARCHAR(100)")
                .HasMaxLength(100);

            builder.Property(d => d.CargaHoraria)
                .IsRequired();

            builder.Property(d => d.Ementa)
                .HasColumnType("VARCHAR(500)")
                .HasMaxLength(500);

            builder.Property(d => d.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            //builder.HasQueryFilter(d => d.Ativo);
        }
    }
}
