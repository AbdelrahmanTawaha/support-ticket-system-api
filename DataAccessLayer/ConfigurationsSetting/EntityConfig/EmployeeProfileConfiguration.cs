using DataAccessLayer.ConfigurationsSetting.Entity;
using DataAccessLayer.ConfigurationsSetting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.ConfigurationsSetting.EntityConfig
{
    public class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
    {
        public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
        {
            builder.ToTable("EmployeeProfiles");


            builder.HasKey(e => e.UserId);

            builder.Property(e => e.EmployeeCode)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(e => e.HireDate)
                   .HasColumnType("date");

            builder.Property(e => e.Salary)
                   .HasColumnType("decimal(18,2)");

            builder.Property(e => e.JobTitle)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(e => e.CreatedOn)
                   .HasColumnType("datetime2");

            builder.Property(e => e.LastModifiedOn)
                   .HasColumnType("datetime2");

            builder.HasOne(e => e.User)
                   .WithOne(u => u.EmployeeProfile)
                   .HasForeignKey<EmployeeProfile>(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.ManagerUser)
         .WithMany(u => u.ManagedEmployees)
         .HasForeignKey(e => e.ManagerUserId)
         .OnDelete(DeleteBehavior.Restrict);




            builder.HasIndex(e => e.ManagerUserId);



            builder.HasData(new EmployeeProfile
            {
                UserId = 1,
                EmployeeCode = "MGR-0001",
                HireDate = new DateTime(2024, 1, 1),
                Salary = 150000,
                JobTitle = EmployeeJobTitle.SupportManager,
                ManagerUserId = null,
                CreatedOn = new DateTime(2025, 12, 16, 0, 0, 0, DateTimeKind.Utc),
                LastModifiedOn = new DateTime(2025, 12, 16, 0, 0, 0, DateTimeKind.Utc),
            });


        }
    }
}
