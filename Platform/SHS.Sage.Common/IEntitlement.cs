using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IEntitlement : IAssociation
    {
        IActor Grantor { get; set; }
        DateTime Issued { get; set; }
        DateTime Expiration { get; set; }
        IPermission[] Permissions { get; set; }
        bool IsDenial { get; set; }
    }
}
