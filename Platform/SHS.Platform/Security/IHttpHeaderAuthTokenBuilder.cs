using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.Security
{
    public interface IHttpHeaderAuthTokenBuilder
    {
        AuthenticationToken GetToken(CinderIdentity identity);
    }
}
