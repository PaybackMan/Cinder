using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Platform.Security.AuthenticationProviders;

namespace Cinder.Platform.Security.AuthProviders
{
    public class CinderAuthenticationProvider : IAuthenticationProvider
    {
        public Task<AuthenticationResponse> AuthenticateAsync(BaseCredentials credentials)
        {
            throw new NotImplementedException();
        }
    }
}
