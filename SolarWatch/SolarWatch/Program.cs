using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SolarWatch.Context;
using SolarWatch.Services.Authentication;
using SolarWatch.Services.CoordinatesApi;
using SolarWatch.Services.JsonProcessor;
using SolarWatch.Services.Repositories;
using SolarWatch.Services.SolarApi;
using SolarWatch.Services.TimeZone;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] { }
        }
    });
});
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<SolarWatchContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<UsersContext>(options => options.UseSqlServer(connectionString));

var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["ValidIssuer"],
            ValidAudience = jwtSettings["ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["IssuerSigningKey"])
            )
        };
    });

builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<UsersContext>();

var rolesSection = builder.Configuration.GetSection("Roles");
var adminRole = rolesSection["Admin"];
var userRole = rolesSection["User"];

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequiredAdminRole", policy => policy.RequireRole(adminRole))
    .AddPolicy("RequiredUserRole", policy => policy.RequireRole(userRole))
    .AddPolicy("RequiredUserOrAdminRole", policy => policy.RequireRole(userRole, adminRole));

builder.Services.AddSingleton<ICityDataProvider, OpenWeatherGeocodingApi>();
builder.Services.AddSingleton<ISolarDataProvider, SolarOrgApi>();
builder.Services.AddSingleton<ITimeZoneQuery, TimeZoneQuery>();
builder.Services.AddSingleton<IJsonProcessor, JsonProcessor>();

builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ISolarDataRepository, SolarDataRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<AuthenticationSeeder>();


var frontendUrl = builder.Configuration.GetValue<string>("frontend_url");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => { builder.WithOrigins(frontendUrl).AllowAnyMethod().AllowAnyHeader(); });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        // Attempt to seed roles and admin user
        var authenticationSeeder = scope.ServiceProvider.GetRequiredService<AuthenticationSeeder>();
        await authenticationSeeder.AddRoles();
        authenticationSeeder.AddAdmin();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeding failed: {ex.Message}. Attempting to apply migrations and retry seeding...");

        try
        {
            // Apply migrations to both contexts
            var dbContexts = new DbContext[]
            {
                scope.ServiceProvider.GetRequiredService<SolarWatchContext>(),
                scope.ServiceProvider.GetRequiredService<UsersContext>()
            };

            foreach (var context in dbContexts)
            {
                context.Database.Migrate();
            }

            Console.WriteLine("Migrations applied successfully. Retrying seeding...");

            // Retry the seeding process after migrations
            var authenticationSeeder = scope.ServiceProvider.GetRequiredService<AuthenticationSeeder>();
            await authenticationSeeder.AddRoles();
            authenticationSeeder.AddAdmin();
            Console.WriteLine("Seeding completed successfully after applying migrations.");
        }
        catch (Exception migrationEx)
        {
            // Log the error if migration and subsequent seeding both fail
            Console.WriteLine($"Migrations and retry of seeding failed: {migrationEx.Message}");
            throw; // Re-throw the exception if needed
        }
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();



public partial class Program { }