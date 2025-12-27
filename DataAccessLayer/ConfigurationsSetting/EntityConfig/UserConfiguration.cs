using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.ConfigurationsSetting.EntityConfig
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FullName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.PhoneNumber)
                   .HasMaxLength(20);

            builder.Property(u => u.UserName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(u => u.UserName)
                   .IsUnique();

            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(u => u.Address)
                   .HasMaxLength(250);

            builder.Property(u => u.ImageUrl)
                   .HasMaxLength(300);


            builder.Property(u => u.UserType)
                   .IsRequired();

            builder.Property(u => u.CreatedAt)
                   .HasColumnType("datetime2");

            builder.Property(u => u.UpdatedAt)
                   .HasColumnType("datetime2");


            builder.HasMany(u => u.CreatedTickets)
                   .WithOne(t => t.Client)
                   .HasForeignKey(t => t.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.AssignedTickets)
                   .WithOne(t => t.AssignedEmployee)
                   .HasForeignKey(t => t.AssignedEmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.HasMany(u => u.Comments)
                   .WithOne(c => c.Author)
                   .HasForeignKey(c => c.AuthorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.EmployeeProfile)
                   .WithOne(e => e.User)
                   .HasForeignKey<EmployeeProfile>(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.ClientProfile)
                   .WithOne(c => c.User)
                   .HasForeignKey<ClientProfile>(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);


            builder.HasData(new User
            {
                Id = 1,
                FullName = "System Manager",
                Email = "manager@demo.com",
                UserName = "manager",
                PasswordHash = "PUT_HASH_HERE",
                PhoneNumber = "0790000000",
                Address = "Amman",
                ImageUrl = null,
                UserType = UserType.SupportManager,
                CreatedAt = new DateTime(2025, 12, 16, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 12, 16, 0, 0, 0, DateTimeKind.Utc),
            });




        }
    }
}
