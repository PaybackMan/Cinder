using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public class _Association : IAssociation
    {
        public virtual string Id
        {
            get; set;
        }

        public IThing Source
        {
            get; set;
        }

        public IThing Target
        {
            get; set;
        }
    }
}
