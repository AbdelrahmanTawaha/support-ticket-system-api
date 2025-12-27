
using DataAccessLayer.ConfigurationsSetting.AiViews;
using DataAccessLayer.ConfigurationsSetting.Entity;
using Microsoft.EntityFrameworkCore;


namespace DataAccessLayer.ConfigurationsSetting
{
    public class AppDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }




        public DbSet<Product> Products { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }

        public DbSet<TicketAiReportRow> TicketAiReportRows { get; set; }

        public DbSet<TicketAiSafeRow> TicketAiSafeRows { get; set; } = null!;

        public DbSet<UserAiSafeRow> UserAiSafeRows { get; set; } = null!;
        public DbSet<ProductAiSafeRow> ProductAiSafeRows { get; set; } = null!;

        public DbSet<ClientProfileAiSafeRow> ClientProfileAiSafeRows { get; set; } = null!;
        public DbSet<EmployeeProfileAiSafeRow> EmployeeProfileAiSafeRows { get; set; } = null!;
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            var config = new ConfigurationBuilder().AddJsonFile("appSetting.json").Build();

            var constr = config.GetSection("constr").Value;

            optionsBuilder.UseSqlServer(constr);
        }
        */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
