using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Runtime
{
    public interface IProxyIdentifiable
    {
        bool IsValid { get; }
        Type IdentifiableType { get; }
        bool CanDeferLoad { get; set; }
        bool IsInitializing { get; set; }
        IRepository Repository { get; }
    }
}
