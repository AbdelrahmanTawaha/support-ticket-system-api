using DataAccessLayer.ConfigurationsSetting.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.ConfigurationsSetting.EntityConfig
{
    public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
    {
        public void Configure(EntityTypeBuilder<TicketAttachment> builder)
        {
            builder.ToTable("TicketAttachments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.FileName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(a => a.FilePath)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(a => a.FileSizeInBytes)
                   .IsRequired();

            builder.Property(a => a.UploadedAt)
                   .HasColumnType("datetime2");

            builder.HasOne(a => a.Ticket)
                   .WithMany(t => t.Attachments)
                   .HasForeignKey(a => a.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);







            builder.HasOne(x => x.UploadedByUser)
             .WithMany()
             .HasForeignKey(x => x.UploadedByUserId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.TicketId);

        }
    }
}
