using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;

namespace SHS.Sage.Linq.Mapping
{
    public class OMappingProperty : IMappedProperty
    {
        public OMappingProperty(PropertyInfo property, string storageName)
        {
            this.Property = property;
            this.StorageProperty = storageName;
        }

        public PropertyInfo Property { get; private set; }

        public string StorageProperty { get; private set; }

        public static OMappingProperty Map<T>(Expression<Func<T, object>> member, string storageName)
        {
            return null;
        }
    }
}
