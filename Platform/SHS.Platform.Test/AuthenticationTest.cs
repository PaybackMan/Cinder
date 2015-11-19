using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernHttpClient;
using SHS.Core.Domain.Security.DocFlock;
using SHS.Platform.RestClients;
using SHS.Platform.Security.AuthProviders.DocFlock;

namespace SHS.Platform.Test
{
    /// <summary>
    /// Summary description for AuthenticationTest
    /// </summary>
    [TestClass]
    public class AuthenticationTest
    {
        public AuthenticationTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void CanAuthenticateDocFlock()
        {
         //   var handler = new NativeMessageHandler();

            var context         = new HttpContext();
            context.HostAddress = new Uri("http://api-shs-dev.docflock.com/");

            //context.HostAddress = new Uri("http://shs-dev.docflock.com/");
          
            var credentials     = new DocFlockCredentials("admin@shs.com", "Password");
            var client          = new HttpClientProvider(context, new CancellationToken());
            var authProvider    = new DocFlockAuthProvider(client);
            var response        = authProvider.AuthenticateAsync(credentials).Result;

           Assert.IsTrue(response.Principal.Identity.IsAuthenticated);
        }
    }
}