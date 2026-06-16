
using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Mappings;

internal class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios", tb => 
        tb.HasCheckConstraint("CK_Usuarios_Role",
            "[Role] IN ('Admin','Coordenador', 'Docente')"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnType("VARCHAR(150)");

        builder.Property(x => x.SenhaHash)
            .IsRequired()
            .HasColumnType("VARCHAR(256)");

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("VARCHAR(20)");

        builder.HasIndex(x => x.Email).IsUnique();
    }
}
