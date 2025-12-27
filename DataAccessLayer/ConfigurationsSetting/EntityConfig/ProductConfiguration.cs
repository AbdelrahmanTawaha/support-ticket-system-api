using DataAccessLayer.ConfigurationsSetting.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.ConfigurationsSetting.EntityConfig
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.Code)
                   .HasMaxLength(50);

            builder.Property(p => p.Description)
                   .HasMaxLength(1000);

            builder.Property(p => p.IsActive)
                   .IsRequired();

            builder.Property(p => p.CreatedAt)
                   .HasColumnType("datetime2");

            builder.Property(p => p.UpdatedAt)
                   .HasColumnType("datetime2");
        }
    }
}
