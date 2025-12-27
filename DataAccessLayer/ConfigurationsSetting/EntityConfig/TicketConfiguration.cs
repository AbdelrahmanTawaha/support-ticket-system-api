using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DataAccessLayer.ConfigurationsSetting.EntityConfig
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("Tickets");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(t => t.Description)
                   .IsRequired()
                   .HasMaxLength(4000);

            builder.Property(t => t.Status)
                   .HasDefaultValue(TicketStatus.New)
                   .IsRequired();

            builder.Property(t => t.CreatedAt)
                   .HasColumnType("datetime2");

            builder.Property(t => t.UpdatedAt)
                   .HasColumnType("datetime2");


            builder.HasOne(t => t.Client)
                   .WithMany(u => u.CreatedTickets)
                   .HasForeignKey(t => t.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(t => t.AssignedEmployee)
                   .WithMany(u => u.AssignedTickets)
                   .HasForeignKey(t => t.AssignedEmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Product
            builder.HasOne(t => t.Product)
                   .WithMany(p => p.Tickets)
                   .HasForeignKey(t => t.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.HasIndex(t => t.ClientId);


            builder.HasIndex(t => t.AssignedEmployeeId);


            builder.HasIndex(t => t.ProductId);







        }
    }
}
