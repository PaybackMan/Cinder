using Orient.Client;
using Orient.Client.Protocol.Serializers;
using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orient.Client.Protocol;
using SHS.Sage.Linq;

namespace SHS.Sage.Data
{
    public class ORecordDeserializer
    {
        const byte AT = 64;  // @
        const byte COLON = 58;  // :
        const byte DOUBLE_QUOTE = 34;  // "
        const byte HASHTAG = 35;  // #
        const byte LEFT_PAREN = 40;  // (
        const byte LEFT_BRACKET = 91;  // [
        const byte LESS_THAN = 60;  // <
        const byte LEFT_CURL = 123; // {
        const byte PERCENT = 37;  // %
        const byte BACKSLASH = 92;  // \
        const byte COMMA = 44;  // ,
        const byte RIGHT_PAREN = 41;  // )
        const byte RIGHT_BRACKET = 93;  // ]
        const byte GREATER_THAN = 62;  // >
        const byte SEMI_COLON = 59;  // ;

        public IEnumerable<KeyValuePair<string, object>> Deserialize(byte[] serializedRecord)
        {
            List<KeyValuePair<string, object>> fields = new List<KeyValuePair<string, object>>();

            int startIndex = serializedRecord.IndexOf(AT);
            int i = serializedRecord.IndexOf(COLON);
            if (i == -1) return fields;

            do
            {
                // parse field name string from raw document string
                string fieldName = ToString(startIndex, i, serializedRecord);

                if (fieldName.StartsWith("@"))
                {
                    fieldName = fieldName.Substring(1, fieldName.Length - 1);
                }

                fieldName = fieldName.Replace(":", ""); // occassionally field names will be suffixed with : for some reason???

                if (fieldName.Equals("rid") || fieldName.Equals("+rid"))
                {
                    fieldName = "Id";
                }

                fieldName = fieldName.Replace("\"", "");

                // move to position after colon (:)
                i++;

                // check if it's not the end of document which means that current field has null value
                if (i == serializedRecord.Length)
                {
                    fields.Add(new KeyValuePair<string, object>(fieldName, null));
                    break; // we're done looping
                }

                object value;
                // check what follows after parsed field name and start parsing underlying type
                switch (serializedRecord[i])
                {
                    case DOUBLE_QUOTE:
                        i = ParseString(i, serializedRecord, out value);
                        break;
                    case HASHTAG:
                        i = ParseRecordID(i, serializedRecord, out value);
                        value = value.ToString();
                        break;
                    case LEFT_PAREN:
                        i = ParseEmbeddedDocument(i, serializedRecord, out value);
                        break;
                    case LEFT_BRACKET:
                        i = ParseList(i, serializedRecord, out value);
                        break;
                    case LESS_THAN:
                        i = ParseSet(i, serializedRecord, out value);
                        break;
                    case LEFT_CURL:
                        i = ParseMap(i, serializedRecord, out value);
                        break;
                    case PERCENT:
                        i = ParseRidBags(i, serializedRecord, out value);
                        break;
                    default:
                        i = ParseValue(i, serializedRecord, out value);
                        break;
                }

                fields.Add(new KeyValuePair<string, object>(fieldName, value));

                // check if it's not the end of document which means that current field has null value
                if (i == serializedRecord.Length)
                {
                    break;
                }

                // single string value was parsed and we need to push the index if next character is comma
                if (serializedRecord[i] == ',')
                {
                    i++;
                }

                startIndex = i;
                i = serializedRecord.IndexOf(COLON, startIndex);
            } while (i < serializedRecord.Length && i > -1);
            return fields;
        }

        private int ParseString(int i, byte[] serializedRecord, out object result)
        {
            // move to the inside of string
            i++;

            int startIndex = i;

            // search for end of the parsed string value
            while (serializedRecord[i] != DOUBLE_QUOTE)
            {
                // strings must escape these characters:
                // " -> \"
                // \ -> \\
                // therefore there needs to be a check for valid end of the string which
                // is quote character that is not preceeded by backslash character \
                if ((serializedRecord[i] == BACKSLASH) && (serializedRecord[i + 1] == DOUBLE_QUOTE))
                {
                    i = i + 2;
                }
                else
                {
                    i++;
                }
            }

            string value = ToString(startIndex, i, serializedRecord);
            // escape quotes
            value = value.Replace("\\" + "\"", "\"");
            // escape backslashes
            value = value.Replace("\\\\", "\\");

            // move past the closing quote character
            i++;
            result = value;
            return i;
        }

        private int ParseValue(int i, byte[] serializedRecord, out object result)
        {
            int startIndex = i;

            // search for end of parsed field value
            while (
                (i < serializedRecord.Length) &&
                (serializedRecord[i] != COMMA) &&
                (serializedRecord[i] != RIGHT_PAREN) &&
                (serializedRecord[i] != RIGHT_BRACKET) &&
                (serializedRecord[i] != GREATER_THAN))
            {
                i++;
            }

            string stringValue = ToString(startIndex, i, serializedRecord);
            object value = new object();

            if (stringValue.Length > 0)
            {
                // binary content
                if ((stringValue.Length > 2) && (stringValue[0] == '_') && (stringValue[stringValue.Length - 1] == '_'))
                {
                    stringValue = stringValue.Substring(1, stringValue.Length - 2);

                    // need to be able for base64 encoding which requires content to be devidable by 4
                    int mod4 = stringValue.Length % 4;

                    if (mod4 > 0)
                    {
                        stringValue += new string('=', 4 - mod4);
                    }

                    value = Convert.FromBase64String(stringValue);
                    value = ReadBase64Array(value);
                }
                // datetime or date
                else if ((stringValue.Length > 2) && (stringValue[stringValue.Length - 1] == 't') || (stringValue[stringValue.Length - 1] == 'a'))
                {
                    // Unix timestamp is miliseconds past epoch
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    string foo = stringValue.Substring(0, stringValue.Length - 1);
                    double d = double.Parse(foo);
                    value = epoch.AddMilliseconds(d);
                    // the date was converted to UTC when stored, but the server includes a UTC conversion when the date is read
                    // effectively doubling the amount of UTC conversion time in the epoch offset provided here
                    // to compensate, we need to subtract 2x the current local time's UTC offset
                    var dtUtc = new DateTime(2000, 1, 1).ToUniversalTime();
                    var dtLocal = new DateTime(2000, 1, 1);
                    value = ((DateTime)value).AddSeconds(2 * dtLocal.Subtract(dtUtc).TotalSeconds);
                }
                // boolean
                else if ((stringValue.Length > 2) && (stringValue == "true") || (stringValue == "false"))
                {
                    value = (stringValue == "true") ? true : false;
                }
                // numbers
                else
                {
                    char lastCharacter = stringValue[stringValue.Length - 1];

                    switch (lastCharacter)
                    {
                        case 'b':
                            value = byte.Parse(stringValue.Substring(0, stringValue.Length - 1));
                            break;
                        case 's':
                            value = short.Parse(stringValue.Substring(0, stringValue.Length - 1));
                            break;
                        case 'l':
                            value = long.Parse(stringValue.Substring(0, stringValue.Length - 1));
                            break;
                        case 'f':
                            value = float.Parse(stringValue.Substring(0, stringValue.Length - 1), CultureInfo.InvariantCulture);
                            break;
                        case 'd':
                            value = double.Parse(stringValue.Substring(0, stringValue.Length - 1), CultureInfo.InvariantCulture);
                            break;
                        case 'c':
                            value = decimal.Parse(stringValue.Substring(0, stringValue.Length - 1), CultureInfo.InvariantCulture);
                            break;
                        default:
                            value = int.Parse(stringValue);
                            break;
                    }
                }
            }
            else if (stringValue.Length == 0)
            {
                value = null;
            }

            //assign field value
            result = value;

            return i;
        }

        private object ReadBase64Array(object value)
        {
            int count = BitConverter.ToInt32((byte[])value, 0);
            Type type = new SHS.Sage.Linq.Language.OStorageType((ODataType)BitConverter.ToInt32((byte[])value, 4)).ToNativeType(); // will only work for intrinsic primitive types
            var array = (Array)Activator.CreateInstance(type.MakeArrayType(), count);
            var idx = 8;
            var read = 0;
            for (int a = 0; a < count; a++)
            {
                array.SetValue(ReadBase64ArrayValue((byte[])value, idx, type, out read), a);
                idx += read;
            }
            return array;
        }

        private object ReadBase64ArrayValue(byte[] source, int idx, Type elementType, out int read)
        {
            var start = idx;
            object ret = null;
            if (elementType.Equals(typeof(string)))
            {
                while (idx < source.Length && source[idx] != 0)
                {
                    idx++;
                }
                ret = UTF8Encoding.UTF8.GetString(source, start, idx - start);
                idx++;
            }
            else if (elementType.Equals(typeof(sbyte)))
            {
                ret = (sbyte)BitConverter.ToChar(source, idx);
                idx += 2;
            }
            else if (elementType.Equals(typeof(byte)))
            {
                ret = source[idx];
                idx += 1;
            }
            else if (elementType.Equals(typeof(bool)))
            {
                ret = BitConverter.ToBoolean(source, idx);
                idx += 1;
            }
            else if (elementType.Equals(typeof(char)))
            {
                ret = BitConverter.ToChar(source, idx);
                idx += 2;
            }
            else if (elementType.Equals(typeof(short)))
            {
                ret = BitConverter.ToInt16(source, idx);
                idx += 2;
            }
            else if (elementType.Equals(typeof(ushort)))
            {
                ret = BitConverter.ToUInt16(source, idx);
                idx += 2;
            }
            else if (elementType.Equals(typeof(int)))
            {
                ret = BitConverter.ToInt32(source, idx);
                idx += 4;
            }
            else if (elementType.Equals(typeof(uint)))
            {
                ret = BitConverter.ToUInt32(source, idx);
                idx += 4;
            }
            else if (elementType.Equals(typeof(long)))
            {
                ret = BitConverter.ToInt64(source, idx);
                idx += 8;
            }
            else if (elementType.Equals(typeof(ulong)))
            {
                ret = BitConverter.ToUInt64(source, idx);
                idx += 8;
            }
            else if (elementType.Equals(typeof(float)))
            {
                ret = BitConverter.ToSingle(source, idx);
                idx += 4;
            }
            else if (elementType.Equals(typeof(double)))
            {
                ret = BitConverter.ToDouble(source, idx);
                idx += 8;
            }
            read = idx - start;
            return ret;
        }

        private int ParseRidBags(int i, byte[] serializedRecord, out object result)
        {
            i++;
            var startIndex = i;
            StringBuilder builder = new StringBuilder();

            while (serializedRecord[i] != SEMI_COLON)
            {
                i++;
            }
            var value = Convert.FromBase64String(ToString(startIndex, i, serializedRecord));
            List<ORID> rids = new List<ORID>();
            using (var ms = new MemoryStream(value))
            {
                using (var br = new BinaryReader(ms))
                {
                    var first = br.ReadByte();
                    int offset = 1;
                    startIndex++;

                    if ((first & 2) == 2)
                    {
                        // uuid parsing is not implemented
                        offset += 16;
                    }

                    if ((first & 1) == 1)
                    {
                        var entriesSize = br.ReadInt32EndianAware();
                        for (int j = 0; j < entriesSize; j++)
                        {
                            var clusterid = br.ReadInt16EndianAware();
                            var clusterposition = br.ReadInt64EndianAware();
                            rids.Add(new ORID(clusterid, clusterposition));
                        }
                    }
                    else throw new NotImplementedException("Tree-based RID Bags are not supported.");
                }
            }

            result = rids;

            i++;
            return i;
        }

        private int ParseMap(int i, byte[] serializedRecord, out object result)
        {
            throw new NotImplementedException();
        }

        private int ParseSet(int i, byte[] serializedRecord, out object result)
        {
            result = null;
            // move to the first element of this set
            i++;

            List<object> results = new List<object>();

            while (serializedRecord[i] != (byte)'>')
            {
                // check what follows after parsed field name and start parsing underlying type
                switch ((char)serializedRecord[i])
                {
                    case '"':
                        i = ParseString(i, serializedRecord, out result);
                        break;
                    case '#':
                        i = ParseRecordID(i, serializedRecord, out result);
                        result = result.ToString();
                        break;
                    case '(':
                        i = ParseEmbeddedDocument(i, serializedRecord, out result);
                        break;
                    case '{':
                        i = ParseMap(i, serializedRecord, out result);
                        break;
                    case ',':
                        i++;
                        results.Add(result);
                        break;
                    default:
                        i = ParseValue(i, serializedRecord, out result);
                        break;
                }
            }
            i++;
            results.Add(result);
            result = results;
            return i;
        }

        private int ParseList(int i, byte[] serializedRecord, out object result)
        {
            result = null;
            // move to the first element of this list
            i++;

            List<object> results = new List<object>();

            while (serializedRecord[i] != (byte)']')
            {
                // check what follows after parsed field name and start parsing underlying type
                switch ((char)serializedRecord[i])
                {
                    case '"':
                        i = ParseString(i, serializedRecord, out result);
                        break;
                    case '#':
                        i = ParseRecordID(i, serializedRecord, out result);
                        result = result.ToString();
                        break;
                    case '(':
                        i = ParseEmbeddedDocument(i, serializedRecord, out result);
                        break;
                    case '{':
                        i = ParseMap(i, serializedRecord, out result);
                        break;
                    case ',':
                        i++;
                        results.Add(result);
                        break;
                    default:
                        i = ParseValue(i, serializedRecord, out result);
                        break;
                }
            }

            i++;
            results.Add(result);
            result = results;
            return i;
        }

        private int ParseEmbeddedDocument(int i, byte[] serializedRecord, out object result)
        {
            result = null;
            i++;

            if ((i < 15) && (serializedRecord.Length > 15) && (UTF8Encoding.UTF8.GetString(serializedRecord, i, 15)).Equals("ORIDs@pageSize:"))
            {
                //var linkCollection = new OLinkCollection();
                //i = ParseLinkCollection(i, recordString, linkCollection);
                //document[fieldName] = linkCollection;
                throw new NotSupportedException();
            }
            else
            {
                var docBytes = new List<byte>();
                while (serializedRecord[i] != (byte)')')
                {
                    docBytes.Add(serializedRecord[i]);
                    i++;
                }
                result = Deserialize(docBytes.ToArray());
            }
            i++;
            return i;
        }

        private int ParseRecordID(int i, byte[] serializedRecord, out object result)
        {
            int startIndex = i;

            // search for end of parsed field value
            while (
                (i < serializedRecord.Length) &&
                (serializedRecord[i] != COMMA) &&
                (serializedRecord[i] != RIGHT_PAREN) &&
                (serializedRecord[i] != RIGHT_BRACKET) &&
                (serializedRecord[i] != GREATER_THAN))
            {
                i++;
            }
            var orid = new ORID(BinarySerializer.ToString(serializedRecord), startIndex);

            result = orid;

            return i;
        }

        private string ToString(int start, int end, byte[] bytes)
        {
            return BinarySerializer.ToString(bytes.Skip(start).Take(end - start).ToArray());
        }
    }
}
