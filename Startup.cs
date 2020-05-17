using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using TauManager.BusinessLogic;
using TauManager.Utils;

namespace TauManager
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<TauDbContext>(options =>
                    options
                        .UseLazyLoadingProxies()
                        .UseNpgsql(Configuration.GetConnectionString("TauDbContextConnection")));

            if (_env.IsProduction())
            {
                var redis = ConnectionMultiplexer
                    .Connect(Configuration.GetConnectionString("Redis"));

                services
                    .AddDataProtection()
                    .PersistKeysToStackExchangeRedis(redis, "DataProtectionKeys");
                
                services.AddStackExchangeRedisCache(option =>
                {
                    option.Configuration = Configuration.GetConnectionString("Redis");
                    option.InstanceName = "TauManagerRedisInstance";
                });
            } else {
                services.AddDistributedMemoryCache();
            }
            
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
            });

            var mvcBuilder = services.AddRazorPages().AddNewtonsoftJson();

#if DEBUG
            if (_env.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }
#endif

            #region Business logic services registration
            services.AddScoped<INotificationLogic, NotificationLogic>();
            services.AddScoped<ICampaignLogic, CampaignLogic>();
            services.AddScoped<IItemLogic, ItemLogic>();
            services.AddScoped<IPlayerLogic, PlayerLogic>();
            services.AddScoped<ILootLogic, LootLogic>();
            services.AddScoped<IUserLogic, UserLogic>();
            services.AddScoped<ISyndicateLogic, SyndicateLogic>();
            services.AddScoped<IMarketLogic, MarketLogic>();
            services.AddScoped<IInternalLogic, InternalLogic>();
            #endregion

            #region Util services registration
            services.AddScoped<ITauHeadClient, TauHead>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            
            app.UseSession();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
