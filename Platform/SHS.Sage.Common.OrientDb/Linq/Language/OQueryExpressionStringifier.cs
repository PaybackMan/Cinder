using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Language
{
    public static class OQueryExpressionStringifier
    {
        public static string ToOrientValueString(this object value, Type targetType = null)
        {
            if (targetType == null)
            {
                if (value == null)
                    targetType = typeof(object);
                else
                    targetType = value.GetType();
            }

            if (value == null && targetType == typeof(object))
                return "null";
            else if (targetType.IsEnum)
            {
                if (value == DBNull.Value) return "null";
                return ((int)value).ToString();
            }
            else if (targetType == typeof(string))
            {
                if (value == DBNull.Value) return "null";
                return string.Format("'{0}'", EscapeText((string)value));
            }
            else if (targetType == typeof(ulong))
            {
                if (value == DBNull.Value) return "null";
                throw new NotSupportedException("Unsigned Long Integer values are not supported.  Use either Decimal, Double or byte[] storage types.");
            }
            else if (targetType == typeof(DateTime))
            {
                if (value == DBNull.Value) return "null";
                var dateString = ((DateTime)value).Kind == DateTimeKind.Utc ?
                    ((DateTime)value).ToString("s").Replace('T', ' ')
                    : ((DateTime)value).ToUniversalTime().ToString("s").Replace('T', ' ');
                return string.Format("'{0}'", dateString);
            }
            else if (targetType.IsArray)
            {
                if (value == DBNull.Value) return "null";
                if (value == null)
                {
                    return "null";
                }
                else if(((Array)value).Rank == 1)
                {
                    if (((Array)value).GetType().GetElementType().IsValueType || ((Array)value).GetType().GetElementType() == typeof(string))
                    {
                        var converter = CreateByteConverter(((Array)value).GetType().GetElementType());
                        var count = ((Array)value).Length;
                        var type = GetElementType(((Array)value).GetType().GetElementType());
                        using (var stream = new MemoryStream())
                        {
                            stream.Write(BitConverter.GetBytes(count), 0, 4);
                            stream.Write(BitConverter.GetBytes(type), 0, 4); // this approach will only work for intrinsic value types
                            byte[] bytes;
                            foreach (var item in (Array)value)
                            {
                                bytes = converter(item);
                                stream.Write(bytes, 0, bytes.Length);
                            }
                            stream.Position = 0;
                            return string.Format("'{0}'", Convert.ToBase64String(stream.ToArray()));
                        }
                    }
                    else if (((Array)value).GetType().GetElementType().Implements<IIdentifiable>())
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < ((Array)value).Length; i++)
                        {
                            var item = ((Array)value).GetValue(i) as IIdentifiable;
                            if (item != null && !string.IsNullOrEmpty(item.Id))
                            {
                                if (sb.Length > 0)
                                    sb.Append(", ");
                                sb.Append(item.Id);
                            }
                        }
                        return sb.Length > 0 ? "[" + sb.ToString() + "]" : "null";
                    }
                }
            }
            else if (targetType == typeof(decimal))
            {
                if (value == DBNull.Value) return "null";
                var sValue = value.ToString();
                if (!sValue.Contains("."))
                    sValue += ".";
                return string.Format("'{0}'", sValue);
            }
            else if (targetType == typeof(double) || targetType == typeof(float))
            {
                if (value == DBNull.Value) return "null";
                return string.Format("'{0}'", value.ToString()); // wrapping in quotes allows orient to parse exponential number formats
            }
            else if (targetType.IsValueType)
            {
                if (value == DBNull.Value) return "null";
                return value.ToString();
            }
            else if (targetType.Implements<IIdentifiable>())
            {
                if (value == DBNull.Value || value == null || string.IsNullOrEmpty(((IIdentifiable)value).Id))
                {
                    return "#-1:-1";
                }
                else
                {
                    return ((IIdentifiable)value).Id;
                }
            }
            else if (targetType.Implements(typeof(IEnumerable<>))
                && targetType.GetGenericArguments()[0].Implements<IIdentifiable>())
            {
                if (value == DBNull.Value) return "null";
                var en = ((IEnumerable)value).GetEnumerator();
                StringBuilder sb = new StringBuilder();
                while (en.MoveNext())
                {
                    var item = en.Current as IIdentifiable;
                    if (item != null && !string.IsNullOrEmpty(item.Id))
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(item.Id);
                    }
                }
                return sb.Length > 0 ? "[" + sb.ToString() + "]" : "null";
            }
            else if (value is DBNull)
            {
                return "null";
            }
            throw new NotSupportedException("Cannot serialize type " + value.GetType().Name + " to a compatible storage format.");
        }

        private static int GetElementType(Type type)
        {
            return new OStorageType(type).ToInt32();
        }

        private static string EscapeText(string value)
        {
            return value.Replace("'", @"\'");
        }

        private static Func<object, byte[]> CreateByteConverter(Type elementType)
        {
            if (elementType.Equals(typeof(sbyte)))
                return (value) => BitConverter.GetBytes((char)value);
            else if (elementType.Equals(typeof(byte)))
                return (value) => new byte[] { (byte)value };
            else if (elementType.Equals(typeof(bool)))
                return (value) => BitConverter.GetBytes((bool)value);
            else if (elementType.Equals(typeof(char)))
                return (value) => BitConverter.GetBytes((char)value);
            else if (elementType.Equals(typeof(short)))
                return (value) => BitConverter.GetBytes((short)value);
            else if (elementType.Equals(typeof(ushort)))
                return (value) => BitConverter.GetBytes((ushort)value);
            else if (elementType.Equals(typeof(int)))
                return (value) => BitConverter.GetBytes((int)value);
            else if (elementType.Equals(typeof(uint)))
                return (value) => BitConverter.GetBytes((uint)value);
            else if (elementType.Equals(typeof(long)))
                return (value) => BitConverter.GetBytes((long)value);
            else if (elementType.Equals(typeof(ulong)))
                return (value) => BitConverter.GetBytes((ulong)value);
            else if (elementType.Equals(typeof(float)))
                return (value) => BitConverter.GetBytes((float)value);
            else if (elementType.Equals(typeof(double)))
                return (value) => BitConverter.GetBytes((double)value);
            else if (elementType.Equals(typeof(string)))
            {
                return (value) =>
                {
                    var list = new List<Byte>();
                    list.AddRange(UTF8Encoding.UTF8.GetBytes((string)value));
                    list.Add(0);
                    return list.ToArray();
                };
            }
            else throw new NotSupportedException("Cannot serialize an array of type " + elementType + ".");
        }
    }
}
