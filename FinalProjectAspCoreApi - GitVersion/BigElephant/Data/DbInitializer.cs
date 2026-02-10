using Microsoft.AspNetCore.Identity;

namespace BigElephant.Data
{
    public static class DbInitializer
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";

        public static async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Admin))
                await roleManager.CreateAsync(new IdentityRole(Admin));

            if (!await roleManager.RoleExistsAsync(Customer))
                await roleManager.CreateAsync(new IdentityRole(Customer));

            var adminEmail = "admin@store.local";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, Admin);
            }
        }
    }
}
