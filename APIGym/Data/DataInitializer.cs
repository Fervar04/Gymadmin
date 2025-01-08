using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace APIGym.Data
{
    public class DataInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public DataInitializer(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task InitializeDataAsync()
        {
            // Crear roles predeterminados
            string[] roles = { "SuperAdmin", "Admin", "Cliente", "Vigilante", "Entrenador" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Crear un usuario SuperAdmin predeterminado si no existe
            var superAdminEmail = "superadmin@gymadminapp.com";
            var superAdminUser = await _userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                superAdminUser = new IdentityUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(superAdminUser, "SuperAdmin123!");
                await _userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }
    }
}