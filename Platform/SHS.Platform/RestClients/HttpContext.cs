using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Cinder.Platform.Security;

namespace Cinder.Platform.RestClients
{
    public class HttpContext
    {
        public HttpContext(string name)
        {
            this.Name = name;
        }
        public string Name { get; set; }
        public Uri HostAddress { get; set; }
        public string ProductName { get; set; }
        public Version Version { get; set; }
        public IPrincipal Principal { get; set; }
    }
}
