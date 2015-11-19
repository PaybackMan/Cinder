using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Platform.Security.Navigation;

namespace Cinder.Platform.Security.AuthenticationProviders
{
    public class AuthenticationResponse
    {
        public bool PasswordExpired { get; set; }
        public CinderPrincipal Principal { get; set; }
     
   
    }
}
