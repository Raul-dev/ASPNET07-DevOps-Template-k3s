using IdentityApi;
using IdentityApi.Data;
using IdentityApi.Extensions;
using IdentityApi.Models;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using static IdentityServer4.Models.IdentityResources;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = LogExtensions.ConfigureLoger()
    .CreateBootstrapLogger();

Log.Information("Starting host...");

builder.Host.UseLogging();

builder.LogStartUp();
/*
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    var kestrelSection = context.Configuration.GetSection("Kestrel");
    serverOptions.Configure(kestrelSection)
        .Endpoint("HTTPS", listenOptions =>
        {
            // ...
        });
});
*/
builder.Services.AddControllersWithViews();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorPages();
/*
var fhOptions = new ForwardedHeadersOptions()
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

fhOptions.KnownProxies.Clear();
fhOptions.KnownNetworks.Clear();
*/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var tempBuilder = builder.Services.AddIdentityServer(options =>
{
    /*
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
    */
    options.EmitStaticAudienceClaim = true;
    
})
    .AddInMemoryIdentityResources(Config.GetIdentityResources(builder.Configuration))
    .AddInMemoryApiScopes(Config.GetApiScopes(builder.Configuration))
    .AddInMemoryApiResources(Config.GetApiResources(builder.Configuration))
    .AddInMemoryClients(Config.GetClients(builder.Configuration))
    .AddAspNetIdentity<ApplicationUser>()
    .AddDeveloperSigningCredential();


//services cors
/*
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
*/

builder.Services.ConfigureExternalCookie(options => {
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = (SameSiteMode)(-1); //SameSiteMode.Unspecified in .NET Core 3.1
});

builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = (SameSiteMode)(-1); //SameSiteMode.Unspecified in .NET Core 3.1
});
var app = builder.Build();

string pathBase = Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
try
{
    using (IServiceScope scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

        Log.Information("Seeding database...");
        var config = builder.Configuration;
        var connectionString = config.GetConnectionString("DefaultConnection");
        SeedData.EnsureSeedData(connectionString);
        Log.Information("Done seeding database.");
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    //app.UseForwardedHeaders(fhOptions);

    if (!string.IsNullOrEmpty(pathBase) && pathBase != "/")
    {
        app.UseStaticFiles(new PathString(pathBase));
        app.UsePathBase(new PathString(pathBase));
        app.UseRouting();
        app.UseIdentityServer();
        Log.Logger.Debug($"Identity subfolder: {pathBase}");
    }
    else
    {
        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
    
        Log.Logger.Debug($"Identity subfolder: Empty");
    }


    //app.UseCors("corsapp");

    app.UseAuthorization();

    app.MapDefaultControllerRoute();
    app.MapRazorPages();

    app.MapGet("/",
        () => "Hello Identity Api!"
    );
    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex.Message);
}
finally
{
    Log.CloseAndFlush();
}
