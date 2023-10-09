using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Recharge_Test.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Recharge_Test
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
            services.AddControllersWithViews();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.AddDbContext<EVDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("EV")));

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var cultures = new List<CultureInfo> {
                    new CultureInfo("vi"),
                    new CultureInfo("en-US")
                };
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("vi");
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });

            services.AddHttpClient();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(p =>
             {
                 p.Cookie.Name = "EVUserCookie";
                 p.ExpireTimeSpan = TimeSpan.FromHours(1);
                 p.LoginPath = "/Auth/Login";
                 p.LogoutPath = "/Auth/Logout";
                 p.ReturnUrlParameter = "returnUrl";
                 p.SlidingExpiration = true;
                 p.Events.OnRedirectToLogin = context =>
                 {
                     context.Response.Redirect(context.RedirectUri + "?returnUrl=" + context.Request.Path);
                     return Task.CompletedTask;
                 };
             })
            .AddExternalCookie();

            services.AddNotyf(config => { config.DurationInSeconds = 10; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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

            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseRouting();

            app.UseNotyf();

            //app.UseSession();

            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Deposit}/{id?}");
            });
        }
    }
}
