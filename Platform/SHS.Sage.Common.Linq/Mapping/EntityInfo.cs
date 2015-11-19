using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Mapping
{
    public struct IdentifiableInfo
    {
        object instance;
        IMappedEntity mapping;

        public IdentifiableInfo(object instance, IMappedEntity mapping)
        {
            this.instance = instance;
            this.mapping = mapping;
        }

        public object Instance
        {
            get { return this.instance; }
        }

        public IMappedEntity Mapping
        {
            get { return this.mapping; }
        }
    }
}
