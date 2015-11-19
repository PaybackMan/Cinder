using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cinder.Platform.Security.AuthProviders.DocFlock
{
   [Newtonsoft.Json.JsonObject(Title = "AccountLoginModel")]
    public class CustomCredentials : BaseCredentials
    {
        public const string Cinder_AUTH_TOKEN_HEADER = "Authorize";
//=======================================================================================================
/// <summary>
/// Default Ctor..
/// </summary>
//=======================================================================================================
        public CustomCredentials()
        {}
//=======================================================================================================
/// <summary>
/// email + password ctor..
/// </summary>
//=======================================================================================================
        public CustomCredentials(string email, string password)
        {
            this.Email    = email;
            this.Password = password;
        }
//=======================================================================================================
/// <summary>
/// Credential related properties..
/// </summary>
//=======================================================================================================
        [JsonProperty]
        public string Email { get; set; }
        [JsonProperty]
        public string Password { get; set; }
//=======================================================================================================
/// <summary>
/// Returns a Dictionary representing the required Http headers for authenticating against said backend.
/// </summary>
/// <returns></returns>
//=======================================================================================================
        public override Dictionary<string, string> GetAuthorizationHeaders()
        {
            var token  = $"CinderAuth Cinderauth_user='{this.Email}', Cinderauth_password='{this.Password}', Cinderauth_version='1.0'";
            return new Dictionary<string, string> {{Cinder_AUTH_TOKEN_HEADER, token}};
        }
    }
}
