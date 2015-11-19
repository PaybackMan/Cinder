using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Mapping
{
    public interface IMappedProperty
    {
        string StorageProperty { get; }
        PropertyInfo Property { get; }
    }
}
