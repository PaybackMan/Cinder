using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Platform.Security.AuthenticationProviders;

namespace Cinder.Platform.Security.AuthProviders
{
    
    public interface IAuthenticationProvider
    {
        Task<AuthenticationResponse> AuthenticateAsync(BaseCredentials credentials);
    }
}
