using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace AdvantageTool
{
    public class IdentityServerConfig
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api1", "My API")
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // machine to machine client
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes = { "api1" }
                },
                
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "96369258-69b4-47da-a6e9-10c25cb2ba88",

                    AllowedGrantTypes = GrantTypes.Code,
                    
                    // where to redirect to after login
                    RedirectUris = { "https://localhost:5002/lti/launch" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId
                    }
                }
            };
    }
}
