using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Language
{
    public enum ODataType : int
    {
        Unsupported = -1,
        Unknown = 0,
        Binary,
        Boolean,
        Decimal,
        Float,
        DateTime,
        Double,
        Integer,
        Link,
        LinkSet,
        Long,
        Short,
        String,
        Char,
        Byte,
    }

    public class OStorageType : StorageType
    {
        Type _type;
        ODataType _dataType;

        public OStorageType(Type csharpType)
        {
            _type = csharpType;
        }

        public OStorageType(ODataType dataType)
        {
            ToInt32();
            _dataType = dataType;
        }

        public override string TypeName
        {
            get
            {
                return _dataType.ToString();
            }
        }

        public override Type ToNativeType() 
        { 
            if (_type == null)
            {
                switch(_dataType)
                {
                    case ODataType.Binary:
                        {
                            return typeof(Array);
                        }
                    case ODataType.Boolean:
                        {
                            return typeof(bool);
                        }
                    case ODataType.Byte:
                        {
                            return typeof(byte);
                        }
                    case ODataType.Char:
                        {
                            return typeof(char);
                        }
                    case ODataType.DateTime:
                        {
                            return typeof(DateTime);
                        }
                    case ODataType.Decimal:
                        {
                            return typeof(Decimal);
                        }
                    case ODataType.Double:
                        {
                            return typeof(Double);
                        }
                    case ODataType.Float:
                        {
                            return typeof(float);
                        }
                    case ODataType.Integer:
                        {
                            return typeof(Int32);
                        }
                    case ODataType.Link:
                        {
                            return typeof(IIdentifiable);
                        }
                    case ODataType.LinkSet:
                        {
                            return typeof(IEnumerable<IIdentifiable>);
                        }
                    case ODataType.Long:
                        {
                            return typeof(long);
                        }
                    case ODataType.Short:
                        {
                            return typeof(short);
                        }
                    case ODataType.String:
                        {
                            return typeof(string);
                        }
                    default: return typeof(object);
                }
            }
            return _type;
        }

        public override int ToInt32()
        {
            if (_dataType == ODataType.Unknown)
            {
                if (_type.Equals(typeof(bool)))
                {
                    _dataType = ODataType.Boolean;
                }
                else if (_type.Equals(typeof(char)))
                {
                    _dataType = ODataType.Char;
                }
                else if (_type.Equals(typeof(byte)))
                {
                    _dataType = ODataType.Byte;
                }
                else if (_type.Equals(typeof(sbyte)))
                {
                    _dataType = ODataType.Short;
                }
                else if (_type.Equals(typeof(short)))
                {
                    _dataType = ODataType.Short;
                }
                else if (_type.Equals(typeof(ushort)))
                {
                    _dataType = ODataType.Integer;
                }
                else if (_type.Equals(typeof(int)))
                {
                    _dataType = ODataType.Integer;
                }
                else if (_type.Equals(typeof(uint)))
                {
                    _dataType = ODataType.Long;
                }
                else if (_type.Equals(typeof(long)))
                {
                    _dataType = ODataType.Long;
                }
                else if (_type.Equals(typeof(ulong)))
                {
                    _dataType = ODataType.Long;
                }
                else if (_type.Equals(typeof(float)))
                {
                    _dataType = ODataType.Float;
                }
                else if (_type.Equals(typeof(double)))
                {
                    _dataType = ODataType.Double;
                }
                else if (_type.Equals(typeof(decimal)))
                {
                    _dataType = ODataType.Decimal;
                }
                else if (_type.Equals(typeof(string)))
                {
                    _dataType = ODataType.String;
                }
                else if (_type.Equals(typeof(DateTime)))
                {
                    _dataType = ODataType.DateTime;
                }
                else if (_type.Implements(typeof(IIdentifiable)))
                {
                    _dataType = ODataType.Link;
                }
                else if (_type.IsEnum)
                {
                    _dataType = ODataType.Integer;
                }
                else if (_type.IsArray)
                {
                    if (_type.GetElementType().IsValueType || _type.GetElementType() == typeof(string))
                    {
                        _dataType = ODataType.Binary;
                    }
                    else if (_type.GetElementType().Implements(typeof(IIdentifiable)))
                    {
                        _dataType = ODataType.LinkSet;
                    }
                }
                else if (_type.Implements(typeof(IEnumerable<>)))
                {
                    var elementType = _type.GetGenericArguments()[0];
                    if (elementType.IsValueType || elementType == typeof(string))
                    {
                        _dataType = ODataType.Binary;
                    }
                    else if (elementType.Implements(typeof(IIdentifiable)))
                    {
                        _dataType = ODataType.LinkSet;
                    }
                }
            }
            return (int)_dataType;
        }
    }
}
