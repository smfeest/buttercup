﻿using System.Globalization;
using Bugsnag.AspNet.Core;
using Buttercup.DataAccess;
using Buttercup.Email;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Buttercup.Web.Infrastructure;
using Buttercup.Web.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Buttercup.Web
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration) => this.configuration = configuration;

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/error");
            }

            app.UseForwardedHeaders();

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new("en-GB"),
                SupportedCultures = new CultureInfo[]
                {
                    new("en-GB"),
                    new("en"),
                    new("fr-FR"),
                    new("fr"),
                },
                SupportedUICultures = new CultureInfo[]
                {
                    new("en-GB"),
                    new("fr"),
                },
            });

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddMvcOptions(options =>
                {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                })
                .AddDataAnnotationsLocalization()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddViewOptions(options =>
                {
                    options.HtmlHelperOptions.ClientValidationEnabled = false;
                });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services
                .AddCoreServices()
                .AddDataAccessServices(this.configuration.GetSection("DataAccess"))
                .AddEmailServices(this.configuration.GetSection("Email"));

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie = new()
                    {
                        Name = "buttercup.auth",
                        HttpOnly = true,
                        SameSite = SameSiteMode.Strict,
                    };
                    options.EventsType = typeof(CookieAuthenticationEventsHandler);
                    options.LoginPath = "/sign-in";
                });

            services.AddBugsnag(
                configuration => configuration.ApiKey = this.configuration["Bugsnag:ApiKey"]);

            services
                .AddTransient<IPasswordHasher<User?>, PasswordHasher<User?>>()
                .AddTransient<IAuthenticationMailer, AuthenticationMailer>()
                .AddTransient<IAuthenticationManager, AuthenticationManager>()
                .AddTransient<CookieAuthenticationEventsHandler>()
                .AddTransient<IRandomNumberGeneratorFactory, RandomNumberGeneratorFactory>()
                .AddTransient<IRandomTokenGenerator, RandomTokenGenerator>()
                .AddTransient<IAssetHelper, AssetHelper>()
                .AddTransient<IAssetManifestReader, AssetManifestReader>()
                .AddSingleton<IAssetManifestSource, AssetManifestSource>()
                .AddTransient<ITimeFormatter, TimeFormatter>()
                .AddTransient<ITimeZoneOptionsHelper, TimeZoneOptionsHelper>()
                .AddTransient<ITimeZoneRegistry, TimeZoneRegistry>();
        }
    }
}
