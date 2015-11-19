using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.Security
{
    public class HttpHeaderAuthTokenBuilder : IHttpHeaderAuthTokenBuilder
    {
        public const string Cinder_AUTH_TOKEN_HEADER = "Authorize";
//==================================================================================================================
/// <summary>
/// 
/// </summary>
//==================================================================================================================
        public HttpHeaderAuthTokenBuilder()
        {}
//==================================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="identity"></param>
/// <returns></returns>
//==================================================================================================================
        public AuthenticationToken GetToken(CinderIdentity identity)
        {
            var token = new AuthenticationToken
            {
                HeaderName = Cinder_AUTH_TOKEN_HEADER,
                Token = $"CinderAuth Cinderauth_user='{identity.Name}', Cinderauth_password='{identity.Password}', Cinderauth_version='1.0'"
            };
            return token;
        }
    }
}
