using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;


namespace AuthServer
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("vds-api","VDS API")
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "vds-client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials.Union(GrantTypes.ResourceOwnerPassword).ToList(),
                    AllowedScopes =
                    {
                        IdentityServer4.IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServer4.IdentityServerConstants.StandardScopes.Profile,
                        IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
                        "vds-api"
                    },
                    ClientSecrets =
                    {
                        new Secret("40A00C685411260BD89DF2459D8EE35FDE2FFAA3AD103EA9CD4362B544CEFE63".Sha256())
                    },
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AllowOfflineAccess = true
                }
            };
        }
    }
}
