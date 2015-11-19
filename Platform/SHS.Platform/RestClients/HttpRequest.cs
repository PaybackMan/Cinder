using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.RestClients
{
    public class HttpRequest
    {
        public string Route { get; set; }
        public string ContentType { get; set; }
        public Dictionary<string, string> ContentHeaders { get; set; }
        public Uri Url { get; set; }

        public HttpRequest(string route)
        {
            this.Route = route;
        }

        public HttpRequest()
        {

        }
    }
}
