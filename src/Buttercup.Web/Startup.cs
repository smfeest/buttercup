using System.Globalization;
using Buttercup.DataAccess;
using Buttercup.Email;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Buttercup.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-GB"),
                SupportedCultures = new[]
                {
                    new CultureInfo("en-GB"),
                    new CultureInfo("en"),
                },
            });

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddViewOptions(options =>
                {
                    options.HtmlHelperOptions.ClientValidationEnabled = false;
                });

            services
                .AddCoreServices()
                .AddDataAccessServices(this.Configuration.GetValue<string>("ConnectionString"))
                .AddEmailServices()
                .Configure<EmailOptions>(this.Configuration.GetSection("Email"));

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie = new CookieBuilder()
                    {
                        Name = "buttercup.auth",
                        HttpOnly = true,
                        SameSite = SameSiteMode.Strict
                    };
                    options.LoginPath = "/sign-in";
                });

            services
                .AddTransient<IPasswordHasher<User>, PasswordHasher<User>>()
                .AddTransient<IAuthenticationManager, AuthenticationManager>()
                .AddTransient<IRandomNumberGeneratorFactory, RandomNumberGeneratorFactory>()
                .AddTransient<IAssetHelper, AssetHelper>()
                .AddTransient<IAssetManifestReader, AssetManifestReader>()
                .AddSingleton<IAssetManifestSource, AssetManifestSource>();
        }
    }
}
