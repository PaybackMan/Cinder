using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Mapping
{
    public abstract class MappingEntity : IMappedEntity
    {
        public abstract string StorageClass { get; set; }
        public abstract Type EntityType { get; }
        public abstract IEnumerable<IMappedProperty> Properties { get; }
        public abstract Type RepositoryType { get; }
    }
}
