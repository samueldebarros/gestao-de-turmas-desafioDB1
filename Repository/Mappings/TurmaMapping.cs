using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Mappings;

public class TurmaMapping : IEntityTypeConfiguration<Turma> 
{
    public void Configure(EntityTypeBuilder<Turma> builder)
    {
        builder.ToTable("Turmas");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Identificador)
            .IsRequired()
            .HasMaxLength(1)
            .HasColumnType("CHAR(1)");

        builder.Property(t => t.AnoLetivo)
            .IsRequired()
            .HasColumnType("SMALLINT");

        builder.Property(t => t.Serie)
            .IsRequired()
            .HasColumnType("TINYINT");

        builder.Property(t => t.VagasMaximas)
            .IsRequired()
            .HasColumnType("TINYINT");

        builder.Property(t => t.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(t => new { t.Identificador, t.AnoLetivo, t.Serie })
            .IsUnique()
            .HasDatabaseName("IX_Turmas_Identificador_AnoLetivo_Serie");

        builder.HasMany(t => t.Enturmamentos)
            .WithOne(e => e.Turma)
            .HasForeignKey(e => e.TurmaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.GradeCurricular)
            .WithOne(g => g.Turma)
            .HasForeignKey(g => g.TurmaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
