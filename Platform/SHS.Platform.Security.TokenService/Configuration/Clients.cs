using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace SHS.Platform.Security.TokenService.Configuration
{
    static class Clients
    {
        public static List<Client> Get()
        {
            return new List<Client>
        {
            // no human involved
            new Client
            {
                ClientName = "Silicon-only Client",
                ClientId = "silicon",
                Enabled = true,
                AccessTokenType = AccessTokenType.Reference,

                Flow = Flows.ClientCredentials,
                ClientSecrets = new List<ClientSecret>
                {
                    new ClientSecret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())
                }
            },

             // human is involved
            new Client
            {
                ClientName = "Silicon on behalf of Carbon Client",
                ClientId = "carbon",
                Enabled = true,
                AccessTokenType = AccessTokenType.Jwt,

                Flow = Flows.ResourceOwner,
                ClientSecrets = new List<ClientSecret>
                {
                    new ClientSecret("21B5F798-BE55-42BC-8AA8-0025B903DC3B".Sha256())
                }
            }


        };
        }
    }

}
