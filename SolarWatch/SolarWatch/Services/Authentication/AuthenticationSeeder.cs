using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Services.Authentication;

public class AuthenticationSeeder
{
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<IdentityUser> userManager;

    public AuthenticationSeeder(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
        _configuration = configuration;
    }

    public void AddRoles()
    {
        var tAdmin = CreateAdminRole(roleManager);
        tAdmin.Wait();

        var tUser = CreateUserRole(roleManager);
        tUser.Wait();
    }

    private async Task CreateAdminRole(RoleManager<IdentityRole> roleManager)
    {
        var adminRole = _configuration["Roles:Admin"];
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    private async Task CreateUserRole(RoleManager<IdentityRole> roleManager)
    {
        var userRole = _configuration["Roles:User"];
        await roleManager.CreateAsync(new IdentityRole(userRole));
    }

    public void AddAdmin()
    {
        var tAdmin = CreateAdminIfNotExsists();
        tAdmin.Wait();
    }

    private async Task CreateAdminIfNotExsists()
    {
        var adminInDb = await userManager.FindByEmailAsync("admin@admin.com");
        if (adminInDb == null)
        {
            var admin = new IdentityUser { UserName = "admin", Email = "admin@admin.com" };
            var adminCreated = await userManager.CreateAsync(admin, "admin123");

            if (adminCreated.Succeeded)
            {
                var adminRole = _configuration["Roles:Admin"];
                await userManager.AddToRoleAsync(admin, adminRole);
            }
        }
    }
}