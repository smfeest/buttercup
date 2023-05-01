using System.Globalization;
using Bugsnag.AspNet.Core;
using Buttercup;
using Buttercup.DataAccess;
using Buttercup.Email;
using Buttercup.EntityModel;
using Buttercup.Web;
using Buttercup.Web.Authentication;
using Buttercup.Web.Infrastructure;
using Buttercup.Web.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

#pragma warning disable CA1852

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
    .AddApiTypes()
    .AddAuthorization()
    .AllowIntrospection(isDevelopment)
    .ModifyRequestOptions(options => options.IncludeExceptionDetails = isDevelopment);

services.Configure<ForwardedHeadersOptions>(
    options => options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

var connectionString = configuration.GetRequiredConnectionString("AppDb");

services
    .AddAppDbContextFactory(connectionString)
    .AddCoreServices()
    .AddDataAccessServices(options => options.ConnectionString = connectionString)
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
    })
    .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>(
        TokenAuthenticationDefaults.AuthenticationScheme, null);

services
    .Configure<Bugsnag.Configuration>(configuration.GetSection("Bugsnag"))
    .AddBugsnag();

services
    .AddTransient<IPasswordHasher<User>, PasswordHasher<User>>()
    .AddTransient<IAccessTokenEncoder, AccessTokenEncoder>()
    .AddTransient<IAccessTokenSerializer, AccessTokenSerializer>()
    .AddTransient<IAuthenticationMailer, AuthenticationMailer>()
    .AddTransient<IAuthenticationManager, AuthenticationManager>()
    .AddTransient<CookieAuthenticationEventsHandler>()
    .AddTransient<IRandomNumberGeneratorFactory, RandomNumberGeneratorFactory>()
    .AddTransient<IRandomTokenGenerator, RandomTokenGenerator>()
    .AddTransient<ITokenAuthenticationService, TokenAuthenticationService>()
    .AddTransient<IUserPrincipalFactory, UserPrincipalFactory>()
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

app.MapGraphQL()
    .WithOptions(new()
    {
        EnableSchemaRequests = isDevelopment,
        Tool = { Enable = isDevelopment }
    })
    .RequireAuthorization(new AuthorizeAttribute
    {
        AuthenticationSchemes = TokenAuthenticationDefaults.AuthenticationScheme
    })
    .AllowAnonymous();

app.Run();
