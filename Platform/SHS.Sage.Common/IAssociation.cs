using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IAssociation : IIdentifiable
    {
        IThing Source { get; set; }
        IThing Target { get; set; }
    }
}
