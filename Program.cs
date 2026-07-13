
using LoginRegistration.Data;
using LoginRegistration.Helpers;
using LoginRegistration.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace LoginRegistration
{
    public class Program
    {
        public static void Main(string[] args)

        {
            DotNetEnv.Env.Load();

           
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();
            // Add services to the container.

            builder.Services.AddControllers();
            //Swagger configuration
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LoginRegistration API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            //jwt settings configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings == null)
            {
                throw new InvalidOperationException("JWT settings are not configured.");
            }
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalDev", policy =>
                {
                    policy.WithOrigins(
      "http://localhost:5173",
      "http://localhost:7116",
      "https://localhost:7116",
      "https://coinage-frontendpage.vercel.app"

  )
  .AllowAnyHeader()
  .AllowAnyMethod();
                });
            }); 
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Add DbContext (replace provider as needed)
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>options.UseSqlServer(
            // builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));


            //builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<GoogleService>();
            builder.Services.AddScoped<LinkedInService>();
            builder.Services.AddScoped<ResumeReaderService>();
            builder.Services.AddScoped<OllamaService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.ConfigObject.PersistAuthorization = true;
                });
            }
            app.UseStaticFiles();
            app.UseCors("AllowLocalDev");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
