using SHS.Sage.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Mapping
{
    public interface IMapEntities : IEnumerable<IMappedEntity>
    {
        Type RepositoryType { get; }
        bool IsInitialized { get; }
        string GetFieldName(IMappedEntity type, MemberInfo member);
        IClass GetClassFromId(string id);
    }
}
