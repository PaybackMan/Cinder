using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Thinktecture.IdentityServer.Core.Logging;

namespace SHS.Platform.Security.TokenService
{
    class Program
    {
        static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());

            using (WebApp.Start<Startup>("https://localhost:44333"))
            {
                Console.WriteLine("server running...");
                Console.ReadLine();
            }

        }
    }
}
