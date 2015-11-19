using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.RestClients
{
    public class HttpStreamRequest : HttpRequest
    {
        public string Filename { get; set; }

        public HttpStreamRequest(string filename)
        {
            Filename = filename;
        }
        
    }
}
