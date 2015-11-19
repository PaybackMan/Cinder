using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.Security.AuthProviders
{
    public abstract class BaseCredentials
    {
        public abstract Dictionary<string, string> GetAuthorizationHeaders();
    }

}
