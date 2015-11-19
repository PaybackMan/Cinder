using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.RestClients
{
    public enum ServiceMessageType
    {
        INFORMATION = 1,
        SECURITY = 2,
        ERROR = 3,
        CONCURRENCY = 4,
        VALIDATION = 5,
        UNKNOWN = 6
    }
}
