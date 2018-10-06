using System.Globalization;
using Buttercup.DataAccess;
using Buttercup.Email;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Buttercup.Web.Infrastructure;
using Buttercup.Web.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
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

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-GB"),
                SupportedCultures = new[]
                {
                    new CultureInfo("en-GB"),
                    new CultureInfo("en"),
                    new CultureInfo("fr-FR"),
                    new CultureInfo("fr"),
                },
                SupportedUICultures = new[]
                {
                    new CultureInfo("en-GB"),
                    new CultureInfo("fr"),
                },
            });

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddDataAnnotationsLocalization()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
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
                .AddTransient<IAuthenticationMailer, AuthenticationMailer>()
                .AddTransient<IAuthenticationManager, AuthenticationManager>()
                .AddTransient<IRandomNumberGeneratorFactory, RandomNumberGeneratorFactory>()
                .AddTransient<IRandomTokenGenerator, RandomTokenGenerator>()
                .AddTransient<IAssetHelper, AssetHelper>()
                .AddTransient<IAssetManifestReader, AssetManifestReader>()
                .AddSingleton<IAssetManifestSource, AssetManifestSource>()
                .AddTransient<ITimeFormatter, TimeFormatter>();
        }
    }
}
