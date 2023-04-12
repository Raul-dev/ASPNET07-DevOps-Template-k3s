using Serilog;
using IdentityServer4;
using IdentityServer4.Models;
using static System.Net.WebRequestMethods;

namespace IdentityApi
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources(IConfiguration config) =>
           new IdentityResource[]
           {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
           };

        public static IEnumerable<ApiScope> GetApiScopes(IConfiguration config) =>
            new ApiScope[]
            {
                
                new ApiScope("catalog.api.read"),
                new ApiScope("order.api.read"),
                new ApiScope("catalog.api.write"),
                new ApiScope("order.api.write"),
                new ApiScope("shopmanager.api.read"),
                new ApiScope("shopmanager.api.write"),
                new ApiScope(name: "full_access", displayName: "Full access to the API")
            };

        public static IEnumerable<ApiResource> GetApiResources(IConfiguration config) =>
            new ApiResource[]
            {
                new ApiResource("IdentityApi")
                {
                    Scopes = {"catalog.api.read", "catalog.api.write", "order.api.read", "order.api.write", "shopmanager.api.read", "shopmanager.api.write" }
                },
                new ApiResource("shop")
                {
                    Scopes = {"catalog.api.read", "order.api.read", "order.api.write"}
                },
            };

        public static IEnumerable<Client> GetClients(IConfiguration config) {
            string secret = config.GetSection("Spa")["Secret"] ?? string.Empty;
            string baseurl = config.GetSection("Spa")["BASE_URL"] ?? string.Empty;
            string schema = config.GetSection("Spa")["Schema"] ?? "http";

            string url = $"{schema}://{baseurl}";

            Log.Debug($"GetClients SPA configuration url={url}");
            string secretCatalog = config.GetSection("Catalog")["Secret"] ?? string.Empty;
            string secretShopManager = config.GetSection("ShopManager")["Secret"] ?? string.Empty;

            return new Client[]
            {
                // interactive client using code flow + pkce
                new Client
                {
                    Enabled = true,
                    ClientId = "mainpage.client.spa",
                    //RequireConsent = false,
                    //RequireClientSecret = false,
                    //RequirePkce = true,

                    ClientSecrets= {new Secret(secret.Sha256())},

                    AllowedGrantTypes = GrantTypes.Code,

                    //RedirectUris = { $"{url}/" },
                    // where to redirect to after login
                    RedirectUris = { $"{url}/signin-oidc"},
            
                    //FrontChannelLogoutUri = $"{url}/signout-oidc",
                    PostLogoutRedirectUris = { $"{url}/authentication/logout-callback" },

                    ClientUri = url,
                    AllowedCorsOrigins = { url },

                    AllowOfflineAccess = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "full_access",
                        "catalog.api.read"
                    }

                },
                
                new Client
                {
                    Enabled = true,

                    ClientId = "catalog.client.api",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret(secretCatalog.Sha256())
                    },

                    AllowedScopes = { "catalog.api.read", "order.api.write" }
                },
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "shopmanager.client.mvc",
                    ClientSecrets = { new Secret(secretShopManager.Sha256()) },
                    Enabled = true,
                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "full_access" }

                }

            };
        }
    }
}