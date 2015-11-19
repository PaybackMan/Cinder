using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cinder.Platform.RestClients
{
    public class MultiPartItem
    {
        [JsonIgnore]
        public byte[] Data { get; set; }
        public string Comments { get; set; }
        public string ContentType { get; set; }
        public string Name { get; set; }
    }

    public class MultiPartData
    {
        public string Client { get; set; }
        public IEnumerable<MultiPartItem> Items { get; set; }
    }
}
