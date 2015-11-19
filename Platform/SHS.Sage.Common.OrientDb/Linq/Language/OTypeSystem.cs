using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Language
{
    public class OTypeSystem : QueryTypeSystem
    {
        public override StorageType Parse(string typeDeclaration)
        {
            return new OStorageType((ODataType)Enum.Parse(typeof(ODataType), typeDeclaration));
        }

        public override StorageType GetStorageType(Type type)
        {
            return new OStorageType(type);
        }

        public override string GetVariableDeclaration(StorageType type, bool suppressSize)
        {
            return ((ODataType)type.ToInt32()).ToString();
        }
    }
}
