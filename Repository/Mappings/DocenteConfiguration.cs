using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

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
            .HasColumnType("VARCHAR(14)")
            .HasMaxLength(14);

        builder.Property(d => d.Email)
            .IsRequired(false)
            .HasColumnType("VARCHAR(150)")
            .HasMaxLength(150);

        builder.Property(d => d.DataNascimento)
            .IsRequired()
            .HasColumnType("DATE");

        builder.Property(d => d.Especialidade)
            .HasColumnType("VARCHAR(50)");

        builder.Property(d => d.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(d => d.Cpf)
            .IsUnique();

        // builder.HasQueryFilter(d => d.Ativo); // comentado para facilitar o teste das funcionalidades
    }
}
