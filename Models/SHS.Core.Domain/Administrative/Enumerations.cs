using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Core.Domain.Administrative
{
    public enum LocationStatus
    {
        /// The location is operational.
        Active,
        /// The location is temporarily closed.
        Suspended,
        /// The location is no longer used.
        Inactive
    }
}
