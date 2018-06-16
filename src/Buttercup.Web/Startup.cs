using System.Globalization;
using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.Web.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddViewOptions(options =>
                {
                    options.HtmlHelperOptions.ClientValidationEnabled = false;
                });

            services.AddDataAccessServices(this.Configuration.GetValue<string>("ConnectionString"));

            services
                .AddTransient<IPasswordHasher<User>, PasswordHasher<User>>()
                .AddTransient<IAssetHelper, AssetHelper>()
                .AddTransient<IAssetManifestReader, AssetManifestReader>()
                .AddSingleton<IAssetManifestSource, AssetManifestSource>();
        }
    }
}
