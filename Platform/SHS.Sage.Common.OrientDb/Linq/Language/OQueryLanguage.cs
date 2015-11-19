using SHS.Sage.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Language
{
    public class OQueryLanguage : QueryLanguage
    {
        OTypeSystem _typeSystem;
        public OQueryLanguage()
        {
           _typeSystem = new OTypeSystem();
        }

        public override QueryTypeSystem TypeSystem
        {
            get { return _typeSystem; }
        }

        public override Expression GetGeneratedIdExpression(MemberInfo member)
        {
            return new FunctionExpression(typeof(string), "@rid", null);
        }

        public override QueryLinguist CreateLinguist(QueryTranslator translator)
        {
            return new OQueryLinguist(this, translator);
        }

        public override bool IsScalar(Type type)
        {
            type = TypeHelper.GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return false;
                case TypeCode.Object:
                    return
                        type == typeof(DateTimeOffset) ||
                        type == typeof(TimeSpan) ||
                        type == typeof(Guid) ||
                        type == typeof(byte[]) ||
                        type.IsArray && IsScalar(type.GetElementType());
                default:
                    return true;
            }
        }
    }
}
