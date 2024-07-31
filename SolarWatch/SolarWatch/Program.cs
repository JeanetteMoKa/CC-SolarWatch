using System.Text;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
        // options.Events.OnForbidden =  context =>
        // {
        //     Console.WriteLine(context.Result.Failure?.Message);
        //     return Task.CompletedTask;
        // };
        // options.Events.OnAuthenticationFailed = context =>
        // {
        //     Console.WriteLine(context.Exception.Message);
        //     return Task.CompletedTask;
        // };
            // options.Events.OnRedirectToLogi = c =>
            // {
            //     c.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //     return Task.FromResult<object>(null);
            // };
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:IssuerSigningKey"])
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
    .AddRoles<IdentityRole>() //Enable Identity roles 
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
builder.Services.AddSingleton<IJsonProcessor, JsonProcessor>();

builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ISolarDataRepository, SolarDataRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<AuthenticationSeeder>();


// Access the configuration directly
var frontendUrl = builder.Configuration.GetValue<string>("frontend_url");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => { builder.WithOrigins(frontendUrl).AllowAnyMethod().AllowAnyHeader(); }); //AllowCredentials()
});

var app = builder.Build();

using var
    scope = app.Services
        .CreateScope(); // AuthenticationSeeder is a scoped service, therefore we need a scope instance to access it
var authenticationSeeder = scope.ServiceProvider.GetRequiredService<AuthenticationSeeder>();
authenticationSeeder.AddRoles();
authenticationSeeder.AddAdmin();

// Configure the HTTP request pipeline.
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