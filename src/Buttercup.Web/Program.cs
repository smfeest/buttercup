using System.Globalization;
using Bugsnag.AspNet.Core;
using Buttercup;
using Buttercup.DataAccess;
using Buttercup.Email;
using Buttercup.Models;
using Buttercup.Web.Api;
using Buttercup.Web.Authentication;
using Buttercup.Web.Infrastructure;
using Buttercup.Web.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

var services = builder.Services;
var configuration = builder.Configuration;

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

services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddTypeExtension<UserExtension>()
    .AllowIntrospection(isDevelopment)
    .ModifyRequestOptions(options => options.IncludeExceptionDetails = isDevelopment);

services.Configure<ForwardedHeadersOptions>(
    options => options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

services
    .AddCoreServices()
    .AddDataAccessServices(configuration.GetSection("DataAccess"))
    .AddEmailServices(configuration.GetSection("Email"));

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

services
    .Configure<Bugsnag.Configuration>(configuration.GetSection("Bugsnag"))
    .AddBugsnag();

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

var app = builder.Build();

if (!isDevelopment)
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
app.MapControllers();
app.MapGraphQL().WithOptions(new()
{
    EnableSchemaRequests = isDevelopment,
    Tool = { Enable = isDevelopment }
});

app.Run();
