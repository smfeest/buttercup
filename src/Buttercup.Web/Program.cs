using Azure.Core;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Bugsnag.AspNet.Core;
using Buttercup.Application;
using Buttercup.Email;
using Buttercup.EntityModel;
using Buttercup.Redis;
using Buttercup.Security;
using Buttercup.Web;
using Buttercup.Web.Api;
using Buttercup.Web.Binders;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Infrastructure;
using Buttercup.Web.Localization;
using Buttercup.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();
var isE2e = builder.Environment.IsEnvironment("E2E");

var services = builder.Services;
var configuration = builder.Configuration;

if (configuration.GetValue<bool>("EnableTelemetry"))
{
    services.AddOpenTelemetry().UseAzureMonitor().WithTracing();
}

services.AddAzureClients(clientBuilder =>
{
    clientBuilder.ConfigureDefaults(configuration.GetSection("Azure:Defaults"));
    clientBuilder.AddEmailClient(configuration.GetSection("Azure:Email"));

    var credentials = new List<TokenCredential>();

    var clientCredentialsSection = configuration.GetSection("Azure:ClientCredentials");
    if (clientCredentialsSection.Exists())
    {
        credentials.Add(
            new ClientSecretCredential(
                clientCredentialsSection["TenantId"],
                clientCredentialsSection["ClientId"],
                clientCredentialsSection["ClientSecret"]));
    }

    credentials.Add(new EnvironmentCredential());

    var managedIdentityClientId = configuration["Azure:ManagedIdentity:ClientId"];
    var managedIdentityId = string.IsNullOrEmpty(managedIdentityClientId) ?
        ManagedIdentityId.SystemAssigned :
        ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId);
    credentials.Add(new ManagedIdentityCredential(managedIdentityId));

    clientBuilder.UseCredential(new ChainedTokenCredential([.. credentials]));
});

services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllersWithViews()
    .AddMvcOptions(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        options.ModelBinderProviders.Insert(0, new NormalizedStringBinderProvider());
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
    .AddDirectiveType<AdminOnlyDirectiveType>()
    .AddErrorInterfaceType<IMutationError>()
    .AddFiltering()
    .AddMutationConventions()
    .AddProjections()
    .AddSorting()
    .AddType<IncorrectCredentialsError>()
    .AddType<TooManyAttemptsError>()
    .DisableIntrospection(!isDevelopment)
    .ModifyOptions(options => options.UseXmlDocumentation = false)
    .ModifyRequestOptions(options => options.IncludeExceptionDetails = isDevelopment)
    .RegisterDbContextFactory<AppDbContext>();

services.Configure<ForwardedHeadersOptions>(
    options => options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

services
    .AddApplicationServices()
    .AddAppDbContextFactory(configuration.GetRequiredConnectionString("AppDb"))
    .AddEmailServices(configuration.GetSection("Email"))
    .AddRedisServices(configuration.GetSection("Redis"))
    .AddSecurityServices(configuration.GetSection("Security"));

services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/access-denied";
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

services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicyNames.AdminOnly, policy => policy.RequireRole(RoleNames.Admin))
    .AddPolicy(
        AuthorizationPolicyNames.AdminOnlyFilterAndSortFields,
        policy => policy.AddRequirements(new AdminOnlyFilterAndSortFieldsRequirement()))
    .AddPolicy(
        AuthorizationPolicyNames.AuthenticatedAndAdminWhenDeleted,
        policy =>
            policy.RequireAuthenticatedUser().AddRequirements(new AdminWhenDeletedRequirement()))
    .AddPolicy(
        AuthorizationPolicyNames.CommentAuthorOrAdmin,
        policy => policy.AddRequirements(new CommentAuthorOrAdminRequirement()))
    .AddPolicy(
        AuthorizationPolicyNames.ParentResultSelfOrAdmin,
        policy => policy.AddRequirements(new ParentResultSelfOrAdminRequirement()));

services
    .Configure<Bugsnag.Configuration>(configuration.GetSection("Bugsnag"))
    .AddBugsnag();

services
    .AddTransient<IInputObjectValidatorFactory, InputObjectValidatorFactory>()
    .AddTransient<IHomeControllerQueries, HomeControllerQueries>()
    .AddTransient<ICommentsControllerQueries, CommentsControllerQueries>()
    .AddTransient<IRecipesControllerQueries, RecipesControllerQueries>()
    .AddTransient<CookieAuthenticationEventsHandler>()
    .AddTransient<IAssetHelper, AssetHelper>()
    .AddTransient<IAssetManifestReader, AssetManifestReader>()
    .AddSingleton<IAssetManifestSource, AssetManifestSource>()
    .AddTransient<ITimeFormatter, TimeFormatter>()
    .AddTransient<ITimeZoneOptionsHelper, TimeZoneOptionsHelper>()
    .AddTransient<ITimeZoneRegistry, TimeZoneRegistry>();

if (isE2e)
{
    services.AddTransient<IDatabaseSeeder, E2eDatabaseSeeder>();
}

var app = builder.Build();

if (!isDevelopment)
{
    app.UseExceptionHandler("/error");
}

app.UseForwardedHeaders();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new("en-GB"),
    SupportedCultures = [new("en-GB"), new("en"), new("fr-FR"), new("fr")],
    SupportedUICultures = [new("en-GB"), new("fr")],
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

if (isE2e)
{
    var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var dbContext = dbContextFactory.CreateDbContext();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
