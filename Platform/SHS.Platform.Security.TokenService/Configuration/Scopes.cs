using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace SHS.Platform.Security.TokenService.Configuration
{
    

    static class Scopes
    {
        public static List<Scope> Get()
        {
            return new List<Scope>
        {
            new Scope
            {
                Name = "SimplicityHealth"
            }
        };
        }
    }

}
