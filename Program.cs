using Labb3CVWeb.Components;
using Labb3CVWeb.Components.Account;
using Labb3CVWeb.Data;
using Labb3CVWeb.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Labb3CVWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ Lägg till Razor Components och Authentication
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            // ✅ Lägg till HttpClient för API-anrop
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://pokeapi.co/") });

            // ✅ Lägg till Authentication & Authorization
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

            // ✅ Hämta API URL från `appsettings.json`
            var apiUrl = builder.Configuration["ApiBaseUrl"];
            if (string.IsNullOrEmpty(apiUrl))
            {
                apiUrl = builder.Environment.IsDevelopment()
                    ? "https://localhost:7204"
                    : builder.Configuration["ApiBaseUrl"];
            }
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

            // ✅ Anslut till backend-databasen
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // ✅ Lägg till Identity med roller
            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; // ⬅️ Ändra till "false" om du inte använder email-verifiering
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";  // ⬅️ Login-sidan
                options.AccessDeniedPath = "/Account/AccessDenied"; // ⬅️ Om användaren saknar behörighet
            });

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.AddScoped<ApiService>();

            // ✅ Lägg till CORS-policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // ✅ Skapa admin-roll vid första start
            using (var scope = app.Services.CreateScope())
            {
                await CreateAdminRole(scope.ServiceProvider, builder.Configuration);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // ✅ Middleware i rätt ordning
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication(); // ⬅️ Måste vara före Authorization
            app.UseAuthorization();  // ⬅️ Måste vara före Antiforgery
            app.UseAntiforgery();    // ⬅️ Flytta hit!
            app.UseCors("AllowAll");

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapAdditionalIdentityEndpoints();

            await app.RunAsync();
        }

        // ✅ Skapa Admin-roll vid första start
        public static async Task CreateAdminRole(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string adminEmail = configuration["AdminUser:Email"] ?? throw new InvalidOperationException("Admin email is missing.");
                string adminPassword = configuration["AdminUser:Password"] ?? throw new InvalidOperationException("Admin password is missing.");

                // ✅ Skapa Admin-roll om den inte finns
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // ✅ Skapa admin-användare om den inte finns
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // ✅ Lägg till Admin-rollen om användaren inte redan har den
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
