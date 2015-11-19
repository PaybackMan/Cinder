using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;

namespace SHS.Platform.Security.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("About to Call test method. Hit a key.");
            Console.ReadLine();

            var token = GetUserToken();
            CallApi(token);
        }

        static void CallApi(TokenResponse response)
        {
            var client = new HttpClient();
            client.SetBearerToken(response.AccessToken);
            Console.WriteLine(client.GetStringAsync("http://localhost:16639/test").Result);
        }

        static TokenResponse GetToken()
        {
            var client = new OAuth2Client(
                new Uri("https://localhost:44333/connect/token"),
                "silicon",
                "F621F470-9731-4A25-80EF-67A6F7C5F4B8");

            return client.RequestClientCredentialsAsync("SimplicityHealth").Result;
        }

        static TokenResponse GetUserToken()
        {
            var client = new OAuth2Client(
                new Uri("https://localhost:44333/connect/token"),
                "carbon",
                "21B5F798-BE55-42BC-8AA8-0025B903DC3B");

            return client.RequestResourceOwnerPasswordAsync("bob", "secret", "SimplicityHealth").Result;
        }



    }
}
