using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Orient.Client;
using SHS.Core.Domain.Administrative;

namespace SHS.Platform.Security.TokenService.Configuration
{
    public class UserService : IUserService
    {
        public Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
        {

            //var repo = new SHSRepository(new ODatabase(CONNECTION_NAME));
            //var parent = repo.Create<Organization>((p) => { p.Name = "ForeMedical"; });

            //repo.SaveChanges();



            //var result = new AuthenticateResult("error");

            // var claim = new Claim("vv", "ee");//claim.

            //result.User.Claims[4].
            //result.HasSubject = true;
            //result.User.
            return null;


        }

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync(ClaimsPrincipal subject)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            throw new NotImplementedException();
        }
    }
}
