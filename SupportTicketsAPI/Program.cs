
using System.Text;
using BusinessLayer.Services.AiAssignSuggest;
using BusinessLayer.Services.Attachment;
using BusinessLayer.Services.DashboardService;
using BusinessLayer.Services.LogginLogout;
using BusinessLayer.Services.PasswordHashService;
using BusinessLayer.Services.Products;
using BusinessLayer.Services.Ticket.implementions;
using BusinessLayer.Services.Ticket.interfaces;
using BusinessLayer.Services.UserManagement;
using DataAccessLayer.ConfigurationsSetting;
using DataAccessLayer.Repositories;
using DataAccessLayer.Repositories.Attachment;
using DataAccessLayer.Repositories.ProductRepository;
using DataAccessLayer.Repositories.ticket.implementions;
using DataAccessLayer.Repositories.ticket.Interface;
using DataAccessLayer.Repositories.user;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using SupportTicketsAPI.Hubs;
using SupportTicketsAPI.Services.AiReport;
using SupportTicketsAPI.Services.Auth;
using SupportTicketsAPI.Services.Email;
using SupportTicketsAPI.Services.Files;




namespace SupportTicketsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.AddControllers();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                var constr = builder.Configuration.GetConnectionString("Connection");
                options.UseSqlServer(constr);



            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateIssuerSigningKey = true,
         ValidateLifetime = true,
         ValidIssuer = jwtSettings.Issuer,
         ValidAudience = jwtSettings.Audience,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
         ClockSkew = TimeSpan.Zero
     };

     options.Events = new JwtBearerEvents
     {
         OnMessageReceived = context =>
         {
             var accessToken = context.Request.Query["access_token"];
             var path = context.HttpContext.Request.Path;

             if (!string.IsNullOrEmpty(accessToken) &&
                 path.StartsWithSegments("/hubs/tickets"))
             {
                 context.Token = accessToken;
             }

             return Task.CompletedTask;
         }
     };
 });
            // Repositories
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
            builder.Services.AddScoped<IClientProfileRepository, ClientProfileRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();





            builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
            builder.Services.Configure<AiSqlOptions>(builder.Configuration.GetSection("AiSql"));

            // Generator via HttpClient
            builder.Services.AddHttpClient<IAiSqlGenerator, GeminiSqlGenerator>();

            // Validator + Executor
            builder.Services.AddScoped<IAiSqlLightValidator, AiSqlLightValidator>();
            builder.Services.AddScoped<IAiReportExecutor, AiReportExecutor>();

            builder.Services.AddHttpClient<IAiAssignSuggestService, AiAssignSuggestService>();


            //=====

            // Services (Business Layer)
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<ITicketAttachmentStorage, TicketAttachmentStorage>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();
            builder.Services.AddScoped<ITicketAttachmentService, TicketAttachmentService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                                   .AllowCredentials();

                    });
            });
            builder.Services.AddSignalR();
            builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));


            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
                .AddDbContextCheck<AppDbContext>(
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "db", "sql" }
                );


            var app = builder.Build();

            app.MapHealthChecks("/health");


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(MyAllowSpecificOrigins);



            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<TicketsHub>("/hubs/tickets");


            app.Run();
        }
    }
}
