using Microsoft.AspNetCore.Identity;

namespace BigElephant.Data
{
    public static class DbInitializer
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Customer = "Customer";

        public static async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // ===== Roles =====
            if (!await roleManager.RoleExistsAsync(SuperAdmin))
                await roleManager.CreateAsync(new IdentityRole(SuperAdmin));

            if (!await roleManager.RoleExistsAsync(Admin))
                await roleManager.CreateAsync(new IdentityRole(Admin));

            if (!await roleManager.RoleExistsAsync(Customer))
                await roleManager.CreateAsync(new IdentityRole(Customer));

            // ===== SuperAdmin (root) =====
            var superEmail = "super@store.local";

            var superUser = await userManager.FindByEmailAsync(superEmail);
            if (superUser == null)
            {
                superUser = new AppUser
                {
                    UserName = superEmail,
                    Email = superEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(superUser, "SuperAdmin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(superUser, SuperAdmin);
            }
            else
            {
                if (!await userManager.IsInRoleAsync(superUser, SuperAdmin))
                    await userManager.AddToRoleAsync(superUser, SuperAdmin);
            }

            // ===== Default Admin =====
            var adminEmail = "admin@store.local";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, Admin);
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, Admin))
                    await userManager.AddToRoleAsync(adminUser, Admin);
            }
        }
    }
}

