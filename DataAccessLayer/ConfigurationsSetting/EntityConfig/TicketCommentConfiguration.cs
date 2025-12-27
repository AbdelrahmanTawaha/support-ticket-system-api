using DataAccessLayer.ConfigurationsSetting.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.ConfigurationsSetting.EntityConfig
{
    public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
    {
        public void Configure(EntityTypeBuilder<TicketComment> builder)
        {
            builder.ToTable("TicketComments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CommentText)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(c => c.CreatedAt)
                   .HasColumnType("datetime2");

            builder.Property(c => c.IsFromSupportTeam)
                   .IsRequired();

            builder.HasOne(c => c.Author)
                   .WithMany(u => u.Comments)
                   .HasForeignKey(c => c.AuthorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Ticket)
                   .WithMany(t => t.Comments)
                   .HasForeignKey(c => c.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(c => c.TicketId);


            builder.HasIndex(c => c.AuthorId);



        }
    }
}
