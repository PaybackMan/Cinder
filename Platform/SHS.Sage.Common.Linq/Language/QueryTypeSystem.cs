using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Language
{
    public abstract class QueryTypeSystem
    {
        public abstract StorageType Parse(string typeDeclaration);
        public abstract StorageType GetStorageType(Type type);
        public abstract string GetVariableDeclaration(StorageType type, bool suppressSize);
    }
}
