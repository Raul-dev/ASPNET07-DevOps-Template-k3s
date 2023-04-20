using CatalogApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.IdentityModel.Logging;
using CatalogApi.Data;
using CatalogApi.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

using System;
using Microsoft.AspNetCore.HttpOverrides;
using System.Runtime.Intrinsics.X86;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = LogExtensions.ConfigureLoger()
        .CreateBootstrapLogger();

builder.Host.UseLogging();
builder.LogStartUp();

builder.Services.AddDbContext<ProductApiContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOptions();
builder.Services.Configure<IdentityS4Settings>(builder.Configuration.GetSection("IdentityS4Settings"));
//string dtr = builder.Configuration.GetSection("IdentityS4Settings")["AuthorityURL"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.Authority = builder.Configuration.GetSection("IdentityS4Settings")["AuthorityURL"];
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidIssuer = builder.Configuration.GetSection("IdentityS4Settings")["IssuerURL"]
        };
    });






builder.Services.AddSwaggerGen(options =>
{

    // Include 'SecurityScheme' to use JWT Authentication
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { jwtSecurityScheme, Array.Empty<string>() }
        });
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Catalog", policy => {
        var scopes = new[] { "catalog.api.read" };
        policy.RequireAssertion(context => {
            var claim = context.User.FindFirst("scope");
            if(claim == null)  return false;
            return claim.Value.Split(' ').Any(s =>
                scopes.Contains(s, StringComparer.Ordinal)
            );
        });
    });

var fhOptions = new ForwardedHeadersOptions()
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost

};

fhOptions.KnownProxies.Clear();
fhOptions.KnownNetworks.Clear();

var app = builder.Build();

IdentityModelEventSource.ShowPII = true;

try
{
    app.UseForwardedHeaders(fhOptions);
    Log.Logger.Debug($"debug 0");
    // Configure the HTTP request pipeline.

    /*
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => {
                Log.Logger.Debug($"Catalog API CertificateCustomValidationCallback: {sender}, {cert}, {chain}, {sslPolicyErrors}  ");
                return true; };
            // Pass the handler to httpclient(from you are calling api)
            HttpClient client = new HttpClient(clientHandler);
    */
    string pathBase = Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
    if (!string.IsNullOrEmpty(pathBase) && pathBase != "/")
    {
        app.UsePathBase(new PathString(pathBase));
        Log.Logger.Debug($"Catalog API subfolder: {pathBase}  ");
        if (!app.Environment.IsProduction())
        {

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    if (!httpRequest.Headers.ContainsKey("X-Forwarded-Host"))
                    {
                        return;
                    }
                    var basePath = pathBase;
                    var serverUrl = $"{httpRequest.Scheme}://{httpRequest.Headers["X-Forwarded-Host"]}{basePath}";
                    swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = serverUrl } };
                });

            });
            app.UseSwaggerUI();
        }
    } 
    else
    {
        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        Log.Logger.Debug($"Catalog API subfolder: Empty");
    }

    if (!app.Environment.IsProduction())
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            Log.Logger.Debug($"Apply miggration.");
            scope.ServiceProvider.GetRequiredService<ProductApiContext>().Database.Migrate();
        }
    } 
    else
    {
        Log.Logger.Debug($"Swagger disabled in production mode.");
    }
    string IsMigration = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (IsMigration == "Migration")
    {
        return;
    }
    


    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/", 
        () => "Catalog API Hello World!")
         .RequireAuthorization("Catalog");

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


