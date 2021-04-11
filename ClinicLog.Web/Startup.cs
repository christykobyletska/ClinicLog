using System.Globalization;
using ClinicLog.Web.Data;
using ClinicLog.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ClinicLog.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            
            services.AddControllersWithViews()
                .AddNewtonsoftJson();
            services.AddRazorPages();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                   IConfigurationSection googleAuthNSection =
                       Configuration.GetSection("Authentication:Google");

                   options.ClientId = googleAuthNSection["ClientId"];
                   options.ClientSecret = googleAuthNSection["ClientSecret"];
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                });
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            PrepareRolesAndUsers(services).Wait();
        }

        private async Task PrepareRolesAndUsers(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            string[] roleNames = 
            {
                Roles.Admin,
                Roles.Doctor,
                Roles.Patient
            };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var powerUserEmail = Configuration.GetSection("UserSettings")["PowerAdminEmail"];
            var powerUser = await userManager.FindByEmailAsync(powerUserEmail);
            if (powerUser == null)
            {
                powerUser = new ApplicationUser
                {
                    FullName = "Power Admin",
                    UserName = powerUserEmail,
                    Email = powerUserEmail
                };
                string userPassword = Configuration.GetSection("UserSettings")["PowerAdminPassword"];
                var createPowerUser = await userManager.CreateAsync(powerUser, userPassword);
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(powerUser, Roles.Admin);
                }
            }

            var defaultDoctorEmail = Configuration.GetSection("UserSettings")["DefaultDoctorEmail"];
            var defaultDoctor = await userManager.FindByEmailAsync(defaultDoctorEmail);
            if (defaultDoctor == null)
            {
                defaultDoctor = new ApplicationUser
                {
                    FullName = "Default Doctor",
                    UserName = defaultDoctorEmail,
                    Email = defaultDoctorEmail
                };
                string userPassword = Configuration.GetSection("UserSettings")["DefaultDoctorPassword"];
                var createDefaultDoctor = await userManager.CreateAsync(defaultDoctor, userPassword);
                if (createDefaultDoctor.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultDoctor, Roles.Doctor);
                }
            }
        }
    }
}
