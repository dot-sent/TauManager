using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TauManager.Areas.Identity.Data;

[assembly: HostingStartup(typeof(TauManager.Areas.Identity.IdentityHostingStartup))]
namespace TauManager.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<TauManagerIdentityDbContext>(options =>
                    options.UseNpgsql(
                        context.Configuration.GetConnectionString("TauManagerIdentityDbContextConnection")));

                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddRoles<IdentityRole>()
                    .AddRoleManager<ApplicationRoleManager>()
                    .AddEntityFrameworkStores<TauManagerIdentityDbContext>()
                    .AddUserManager<ApplicationIdentityUserManager>()
                    .AddDefaultUI();
            });
        }
    }
}