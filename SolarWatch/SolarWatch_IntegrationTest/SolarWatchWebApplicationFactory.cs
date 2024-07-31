using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarWatch.Context;
using SolarWatch.Model.DbModel;

namespace SolarWatch_IntegrationTest;

 public class SolarWatchWebApplicationFactory : WebApplicationFactory<Program>
  {
     // Create a new db name for each SolarWatchWebApplicationFactory. This is to prevent tests failing from changes done in db by a previous test. 
      private readonly string _dbName = Guid.NewGuid().ToString();

      protected override void ConfigureWebHost(IWebHostBuilder builder)
      {
          builder.UseEnvironment("Testing");
          builder.ConfigureServices(services =>
          {
              //Get the previous DbContextOptions registrations 
              var solarWatchDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SolarWatchContext>));
              var usersDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UsersContext>));
              
              //Remove the previous DbContextOptions registrations
              services.Remove(solarWatchDbContextDescriptor);
              services.Remove(usersDbContextDescriptor);
              
              //Add new DbContextOptions for our two contexts, this time with inmemory db 
              services.AddDbContext<SolarWatchContext>(options =>
              {
                  options.UseInMemoryDatabase(_dbName);
              });
              
              services.AddDbContext<UsersContext>(options =>
              {
                  options.UseInMemoryDatabase(_dbName);
              });
              
              //We will need to initialize our in memory databases. 
              //Since DbContexts are scoped services, we create a scope
              using var scope = services.BuildServiceProvider().CreateScope();
              
              //We use this scope to request the registered dbcontexts, and initialize the schemas
              var solarContext = scope.ServiceProvider.GetRequiredService<SolarWatchContext>();
              var userContext = scope.ServiceProvider.GetRequiredService<UsersContext>();
              
              solarContext.Database.EnsureDeleted();
              solarContext.Database.EnsureCreated();

              userContext.Database.EnsureDeleted();
              userContext.Database.EnsureCreated();

              //Here we could do more initializing if we wished (e.g. adding admin user)
              
              // Seed SolarData
              solarContext.SolarTimes.Add(new SolarData
              {
                  City = new City
                  {
                      Name = "Budapest",
                      Country = "Hungary",
                      Latitude = 47.4979937,
                      Longitude = 19.0403594
                  },
                  Sunrise = new DateTime(2024, 7, 29, 5, 18, 43),
                  Sunset = new DateTime(2024, 7, 29, 20, 22, 15)
              });
              solarContext.SaveChanges();

              // Seed Identity data
              var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
              var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
              
              // Add roles
              var roles = new[] { "Admin", "User" };
              foreach (var role in roles)
              {
                  if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                  {
                      roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                  }
              }

              // Add users
              var adminUser = new IdentityUser { UserName = "admin", Email = "admin@admin.com" };
              var userCreationResult = userManager.CreateAsync(adminUser, "Password123!").GetAwaiter().GetResult();
              
              var normalUser = new IdentityUser { UserName = "tesztelek", Email = "teszt@teszt.com" };
              var normalUserCreationResult = userManager.CreateAsync(normalUser, "asd123").GetAwaiter().GetResult();
              if (userCreationResult.Succeeded)
              {
                  userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
              }
              if (normalUserCreationResult.Succeeded)
              {
                  userManager.AddToRoleAsync(normalUser, "User").GetAwaiter().GetResult();
              }

              // Optionally add more users and roles as needed
          });
      }
}
