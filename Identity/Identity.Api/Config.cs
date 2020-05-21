using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Api
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetResources()
        {
            return new List<ApiResource>()
            {
                //new ApiResource("api","User Api"),
                new ApiResource("gateway_api","GateWay Api"),
            };
        }

        internal static IEnumerable<Client> GetClients()
        {
            return new List<Client>()
            {
                //new Client()
                //{
                //    ClientId = "UserClient",
                //    AllowedGrantTypes = GrantTypes.ClientCredentials,
                //    ClientSecrets = new List<Secret>()
                //    {
                //        new Secret("HaussSecret".Sha256())
                //    },
                //    AllowedScopes = { "api" }
                //},
                new Client()
                {
                    ClientId = "GateWayClient",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = new List<Secret>()
                    {
                        new Secret("HaussSecret".Sha256())
                    },
                    AllowedScopes = { "gateway_api" }
                }
            };
        }
    }
}
