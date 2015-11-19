using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public class IdentifiableAlias
    {
        public IdentifiableAlias()
        {
        }

        public override string ToString()
        {
            return "A:" + this.GetHashCode();
        }
    }
}
