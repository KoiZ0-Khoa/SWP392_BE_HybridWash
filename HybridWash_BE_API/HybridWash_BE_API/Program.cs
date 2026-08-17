using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
// TODO: UNCOMMENT cÃ¡c using sau khi cháº¡y lá»‡nh EF migration xong
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Implementations;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.Implementations;
using HybridWash.Services.Interfaces;
using HybridWash_BE_API.Security;
using Microsoft.EntityFrameworkCore;
using HybridWash.Services.BackgroundServices;

namespace HybridWash_BE_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // ======================================================================
            // TODO: UNCOMMENT cÃ¡c dÃ²ng DI sau khi cháº¡y lá»‡nh EF migration xong
            // LÃ½ do comment: Models + Data (AutowashContext) Ä‘Ã£ bá»‹ xÃ³a Ä‘á»ƒ cáº­p nháº­t DB.
            // ======================================================================

            // Configure DbContext
            builder.Services.AddDbContext<AutowashContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("MyCnn"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<HybridWash.Repositories.Interfaces.IStaffRepository, HybridWash.Repositories.Implementations.StaffRepository>();
            builder.Services.AddScoped<HybridWash.Services.Interfaces.IStaffService, HybridWash.Services.Implementations.StaffService>();

            builder.Services.AddScoped<HybridWash.Repositories.Interfaces.ICustomerRepository, HybridWash.Repositories.Implementations.CustomerRepository>();
            builder.Services.AddScoped<HybridWash.Services.Interfaces.ICustomerService, HybridWash.Services.Implementations.CustomerService>();

            // api booking
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddScoped<IServiceService, ServiceService>();

            builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
            builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();

            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IBookingImageService, BookingImageService>();
            builder.Services.AddScoped<IIncidentReportRepository, IncidentReportRepository>();
            builder.Services.AddScoped<IIncidentReportService, IncidentReportService>();
            builder.Services.AddScoped<IIncidentReportImageService, IncidentReportImageService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ISystemParameterRepository, SystemParameterRepository>();
            builder.Services.AddScoped<ISystemParameterService, SystemParameterService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();

            // AWS S3
            // AccessKeyId/SecretAccessKey are stored in .NET User Secrets for local development.
            // If they are not configured, the AWS SDK falls back to its default credential chain
            // (environment variables, AWS profile, IAM role, etc.).
            var awsOptions = builder.Configuration.GetAWSOptions();
            var awsAccessKeyId = builder.Configuration["AWS:AccessKeyId"];
            var awsSecretAccessKey = builder.Configuration["AWS:SecretAccessKey"];

            if (!string.IsNullOrWhiteSpace(awsAccessKeyId) &&
                !string.IsNullOrWhiteSpace(awsSecretAccessKey))
            {
                awsOptions.Credentials = new BasicAWSCredentials(
                    awsAccessKeyId,
                    awsSecretAccessKey);
            }

            builder.Services.AddDefaultAWSOptions(awsOptions);
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddScoped<IAwsS3Service, AwsS3Service>();

            builder.Services.AddHttpClient<IPlateOcrService, OcrSpacePlateOcrService>();

            // Register Background Service for auto-cleanup
            builder.Services.AddHostedService<BookingCleanupBackgroundService>();

            builder.Services.AddMemoryCache();

            // Background Service for Washing Automation

            builder.Services.AddHostedService<HybridWash.Services.BackgroundServices.MonthlyTierReviewService>();

            builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
            builder.Services.AddLoyaltyModule();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HybridWash API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter the JWT token only. Swagger will add the Bearer prefix automatically.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            // Configure JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForHybridWashWhichIsAtLeast32BytesLong!";
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });

            var app = builder.Build();

            // Apply pending EF Core migrations without recreating the existing database.
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AutowashContext>();
                context.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            // Disable HTTPS redirection for local mobile development
            // app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
