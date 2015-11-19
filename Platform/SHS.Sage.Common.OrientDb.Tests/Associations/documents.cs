using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.OrientDb.Tests.Associations
{
    public class documents : IAssociation
    {
        public string Id { get; set; }

        public IThing Source { get; set; }

        public IThing Target { get; set; }

        public string Type { get; set; }
    }
}
