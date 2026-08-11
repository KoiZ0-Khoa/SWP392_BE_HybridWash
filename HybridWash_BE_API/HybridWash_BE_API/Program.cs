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
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddScoped<IServiceService, ServiceService>();

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
