using DataAccessLayer.ConfigurationsSetting.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.ConfigurationsSetting.EntityConfig
{
    public class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
    {
        public void Configure(EntityTypeBuilder<ClientProfile> builder)
        {
            builder.ToTable("ClientProfiles");

            builder.HasKey(c => c.UserId);

            builder.Property(c => c.CompanyName)
                   .HasMaxLength(200);

            builder.Property(c => c.CompanyAddress)
                   .HasMaxLength(300);

            builder.Property(c => c.VatNumber)
                   .HasMaxLength(50);

            builder.Property(c => c.PreferredLanguage)
                   .HasMaxLength(10);

            builder.Property(c => c.CreatedOn)
                   .HasColumnType("datetime2");

            builder.Property(c => c.LastModifiedOn)
                   .HasColumnType("datetime2");

            builder.HasOne(c => c.User)
                   .WithOne(u => u.ClientProfile)
                   .HasForeignKey<ClientProfile>(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
