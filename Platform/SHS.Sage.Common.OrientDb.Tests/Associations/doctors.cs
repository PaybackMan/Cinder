using SHS.Sage.OrientDb.Tests.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.OrientDb.Tests.Associations
{
    public class doctors : IAssociation
    {
        public string Id { get; set; }
        public virtual IThing Source { get; set; }
        public virtual IThing Target { get; set; }
        public virtual Patient PreferredPatient { get; set; }
    }
}
