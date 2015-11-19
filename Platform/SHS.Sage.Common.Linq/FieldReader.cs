using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public abstract class FieldReader
    {
        //KeyValuePair<string, TypeCode>[] typeCodes;

        public FieldReader()
        {
        }

        protected void Init()
        {
        }

        protected abstract int GetOrdinal(string name);

        protected abstract int FieldCount { get; }
        protected abstract Type GetFieldType(int ordinal);
        protected abstract bool IsDBNull(int ordinal);
        protected abstract T GetValue<T>(int ordinal);
        protected abstract Byte GetByte(int ordinal);
        protected abstract Char GetChar(int ordinal);
        protected abstract DateTime GetDateTime(int ordinal);
        protected abstract Decimal GetDecimal(int ordinal);
        protected abstract Double GetDouble(int ordinal);
        protected abstract Single GetSingle(int ordinal);
        protected abstract Guid GetGuid(int ordinal);
        protected abstract Int16 GetInt16(int ordinal);
        protected abstract Int32 GetInt32(int ordinal);
        protected abstract Int64 GetInt64(int ordinal);
        protected abstract String GetString(int ordinal);

        protected abstract Type GetFieldType(string name);
        protected abstract bool IsDBNull(string name);
        protected abstract T GetValue<T>(string name);
        protected abstract Byte GetByte(string name);
        protected abstract Char GetChar(string name);
        protected abstract DateTime GetDateTime(string name);
        protected abstract Decimal GetDecimal(string name);
        protected abstract Double GetDouble(string name);
        protected abstract Single GetSingle(string name);
        protected abstract Guid GetGuid(string name);
        protected abstract Int16 GetInt16(string name);
        protected abstract Int32 GetInt32(string name);
        protected abstract Int64 GetInt64(string name);
        protected abstract String GetString(string name);

        public T ReadValue<T>(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(T);
            }
            return this.GetValue<T>(ordinal);
        }

        public T? ReadNullableValue<T>(int ordinal) where T : struct
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(T?);
            }
            return this.GetValue<T>(ordinal);
        }

        public Byte ReadByte(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Byte);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Byte)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Byte)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Byte)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Byte)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Byte)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Byte)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Byte>(ordinal);
            }
        }

        public Byte? ReadNullableByte(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Byte?);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Byte)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Byte)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Byte)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Byte)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Byte)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Byte)this.GetDecimal(ordinal);
                default:
                    return (Byte)this.GetValue<Byte>(ordinal);
            }
        }

        public Char ReadChar(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Char);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Char)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Char)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Char)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Char)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Char)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Char)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Char)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Char>(ordinal);
            }
        }

        public Char? ReadNullableChar(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Char?);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Char)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Char)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Char)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Char)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Char)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Char)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Char)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Char>(ordinal);
            }
        }

        public DateTime ReadDateTime(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(DateTime);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.DateTime:
                    return this.GetDateTime(ordinal);
                default:
                    return this.GetValue<DateTime>(ordinal);
            }
        }

        public DateTime? ReadNullableDateTime(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(DateTime?);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.DateTime:
                    return this.GetDateTime(ordinal);
                default:
                    return this.GetValue<DateTime>(ordinal);
            }
        }

        public Decimal ReadDecimal(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Decimal);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Decimal)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Decimal)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Decimal)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Decimal)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Decimal)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Decimal)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Decimal>(ordinal);
            }
        }

        public Decimal? ReadNullableDecimal(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Decimal?);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Decimal)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Decimal)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Decimal)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Decimal)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Decimal)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Decimal)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Decimal>(ordinal);
            }
        }

        public Double ReadDouble(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Double);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Double)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Double)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Double)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Double)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Double)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Double)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Double>(ordinal);
            }
        }

        public Double? ReadNullableDouble(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Double?);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Double)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Double)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Double)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Double)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Double)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Double)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Double>(ordinal);
            }
        }

        public Single ReadSingle(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Single);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Single)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Single)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Single)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Single)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Single)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Single)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Single>(ordinal);
            }
        }

        public Single? ReadNullableSingle(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Single?);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Single)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Single)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Single)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Single)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Single)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Single)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Single>(ordinal);
            }
        }

        public Guid ReadGuid(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Guid);
            }

            switch (GetTypeCode(ordinal))
            {
                case tcGuid:
                    return this.GetGuid(ordinal);
                default:
                    return this.GetValue<Guid>(ordinal);
            }
        }

        public Guid? ReadNullableGuid(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Guid?);
            }

            switch (GetTypeCode(ordinal))
            {
                case tcGuid:
                    return this.GetGuid(ordinal);
                default:
                    return this.GetValue<Guid>(ordinal);
            }
        }

        public Int16 ReadInt16(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Int16);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Int16)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Int16)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Int16)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Int16)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Int16)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Int16)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Int16)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Int16>(ordinal);
            }
        }

        public Int16? ReadNullableInt16(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Int16?);
            }
           
            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Int16)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Int16)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Int16)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Int16)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Int16)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Int16)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Int16)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Int16>(ordinal);
            }
        }

        public Int32 ReadInt32(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Int32);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Int32)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Int32)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Int32)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Int32)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Int32)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Int32)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Int32)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Int32>(ordinal);
            }
        }

        public Int32? ReadNullableInt32(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Int32?);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Int32)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Int32)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Int32)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Int32)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Int32)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Int32)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Int32)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Int32>(ordinal);
            }
        }

        public Int64 ReadInt64(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Int64);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Int64)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Int64)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Int64)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Int64)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Int64)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Int64)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Int64)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Int64>(ordinal);
            }
        }

        public Int64? ReadNullableInt64(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Int64?);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return (Int64)this.GetByte(ordinal);
                case TypeCode.Int16:
                    return (Int64)this.GetInt16(ordinal);
                case TypeCode.Int32:
                    return (Int64)this.GetInt32(ordinal);
                case TypeCode.Int64:
                    return (Int64)this.GetInt64(ordinal);
                case TypeCode.Double:
                    return (Int64)this.GetDouble(ordinal);
                case TypeCode.Single:
                    return (Int64)this.GetSingle(ordinal);
                case TypeCode.Decimal:
                    return (Int64)this.GetDecimal(ordinal);
                default:
                    return this.GetValue<Int64>(ordinal);
            }
        }

        public String ReadString(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(String);
            }
            
            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return this.GetByte(ordinal).ToString();
                case TypeCode.Int16:
                    return this.GetInt16(ordinal).ToString();
                case TypeCode.Int32:
                    return this.GetInt32(ordinal).ToString();
                case TypeCode.Int64:
                    return this.GetInt64(ordinal).ToString();
                case TypeCode.Double:
                    return this.GetDouble(ordinal).ToString();
                case TypeCode.Single:
                    return this.GetSingle(ordinal).ToString();
                case TypeCode.Decimal:
                    return this.GetDecimal(ordinal).ToString();
                case TypeCode.DateTime:
                    return this.GetDateTime(ordinal).ToString();
                case tcGuid:
                    return this.GetGuid(ordinal).ToString();
                case TypeCode.String:
                    return this.GetString(ordinal);
                default:
                    return this.GetValue<String>(ordinal);
            }
        }

        public Byte[] ReadByteArray(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Byte[]);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Byte:
                    return new Byte[] { this.GetByte(ordinal) };
                default:
                    return this.GetValue<Byte[]>(ordinal);
            }
        }

        public Char[] ReadCharArray(int ordinal)
        {
            Init();
            if (this.IsDBNull(ordinal))
            {
                return default(Char[]);
            }

            switch (GetTypeCode(ordinal))
            {
                case TypeCode.Char:
                    return new Char[] { this.GetChar(ordinal) };
                default:
                    return this.GetValue<Char[]>(ordinal);
            }
        }



        public T ReadValue<T>(string name)
        {
            Init();
            if (this.IsDBNull(name))
            {
                return default(T);
            }
            return this.GetValue<T>(name);
        }

        public T? ReadNullableValue<T>(string name) where T : struct
        {
            Init();
            if (this.IsDBNull(name))
            {
                return default(T?);
            }
            return this.GetValue<T>(name);
        }

        public Byte ReadByte(string name)
        {
            Init();
            try
            {
                return ReadByte(GetOrdinal(name));
            }
            catch
            {
                return 0;
            }
        }

        public Byte? ReadNullableByte(string name)
        {
            Init();
            try
            {
                return ReadNullableByte(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Char ReadChar(string name)
        {
            Init();
            try
            {
                return ReadChar(GetOrdinal(name));
            }
            catch
            {
                return (char)0;
            }
        }

        public Char? ReadNullableChar(string name)
        {
            Init();
            try
            {
                return ReadNullableChar(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public DateTime ReadDateTime(string name)
        {
            Init();
            try
            {
                return ReadDateTime(GetOrdinal(name));
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public DateTime? ReadNullableDateTime(string name)
        {
            Init();
            try
            {
                return ReadNullableDateTime(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Decimal ReadDecimal(string name)
        {
            Init();
            try
            {
                return ReadDecimal(GetOrdinal(name));
            }
            catch
            {
                return 0;
            }
        }

        public Decimal? ReadNullableDecimal(string name)
        {
            Init();
            try
            {
                return ReadNullableDecimal(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Double ReadDouble(string name)
        {
            Init();
            try
            {
                return ReadDouble(GetOrdinal(name));
            }
            catch
            {
                return 0;
            }
        }

        public Double? ReadNullableDouble(string name)
        {
            Init();
            try
            {
                return ReadNullableDouble(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Single ReadSingle(string name)
        {
            Init();
            try
            {
                return ReadSingle(GetOrdinal(name));
            }
            catch
            {
                return 0;
            }
        }

        public Single? ReadNullableSingle(string name)
        {
            Init();
            try
            {
                return ReadNullableSingle(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Guid ReadGuid(string name)
        {
            Init();
            try
            {
                return ReadGuid(GetOrdinal(name));
            }
            catch
            {
                return Guid.Empty;
            }
        }

        public Guid? ReadNullableGuid(string name)
        {
            Init();
            try
            {
                return ReadNullableGuid(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Int16 ReadInt16(string name)
        {
            Init();
            try
            {
                return ReadInt16(GetOrdinal(name));
            }
            catch
            {
                return 0;
            }
        }

        public Int16? ReadNullableInt16(string name)
        {
            Init();
            try
            {
                return ReadNullableInt16(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Int32 ReadInt32(string name)
        {
            Init();
            try
            {
                return ReadInt32(GetOrdinal(name));
            }
            catch
            {
                return 0;
            }
        }

        public Int32? ReadNullableInt32(string name)
        {
            Init();
            try
            {
                return ReadNullableInt32(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Int64 ReadInt64(string name)
        {
            Init();
            try
            {
                return ReadInt64(GetOrdinal(name));
            }
            catch
            {
                return 0;
            }
        }

        public Int64? ReadNullableInt64(string name)
        {
            Init();
            try
            {
                return ReadNullableInt64(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public String ReadString(string name)
        {
            Init();
            try
            {
                return ReadString(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Byte[] ReadByteArray(string name)
        {
            Init();
            try
            {
                return ReadByteArray(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        public Char[] ReadCharArray(string name)
        {
            Init();
            try
            {
                return ReadCharArray(GetOrdinal(name));
            }
            catch
            {
                return null;
            }
        }

        private const TypeCode tcGuid = (TypeCode)20;
        private const TypeCode tcByteArray = (TypeCode)21;
        private const TypeCode tcCharArray = (TypeCode)22;

        protected virtual TypeCode GetTypeCode(int ordinal)
        {
            Init();
            Type type = this.GetFieldType(ordinal);
            TypeCode tc = Type.GetTypeCode(type);
            if (tc == TypeCode.Object)
            {
                if (type == typeof(Guid))
                    tc = tcGuid;
                else if (type == typeof(Byte[]))
                    tc = tcByteArray;
                else if (type == typeof(Char[]))
                    tc = tcCharArray;
            }
            return tc;
        }

        private TypeCode GetTypeCode(string name)
        {
            Init();
            Type type = this.GetFieldType(name);
            TypeCode tc = Type.GetTypeCode(type);
            if (tc == TypeCode.Object)
            {
                if (type == typeof(Guid))
                    tc = tcGuid;
                else if (type == typeof(Byte[]))
                    tc = tcByteArray;
                else if (type == typeof(Char[]))
                    tc = tcCharArray;
            }
            return tc;
        }

        public static MethodInfo GetReaderMethod(Type type, bool useOrindal)
        {
            if (useOrindal)
            {
                lock(_syncRoot)
                {
                    if (_ordinalReaderMethods == null)
                    {
                        var meths = typeof(FieldReader).GetMethods(
                            BindingFlags.Public | BindingFlags.Instance)
                            .Where(m => m.Name.StartsWith("Read")
                            && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType.Equals(typeof(int)))
                            .ToList();
                        _ordinalReaderMethods = meths.ToDictionary(m => m.ReturnType);
                        _miOrdinalReadValue = meths.Single(m => m.Name == "ReadValue");
                        _miOrdinalReadNullableValue = meths.Single(m => m.Name == "ReadNullableValue");
                    }
                }

                MethodInfo mi;
                _ordinalReaderMethods.TryGetValue(type, out mi);
                if (mi == null)
                {
                    if (TypeHelper.IsNullableType(type))
                    {
                        mi = _miOrdinalReadNullableValue.MakeGenericMethod(TypeHelper.GetNonNullableType(type));
                    }
                    else
                    {
                        mi = _miOrdinalReadValue.MakeGenericMethod(type);
                    }
                }
                return mi;
            }
            else
            {
                lock(_syncRoot)
                {
                    if (_namedReaderMethods == null)
                    {
                        var meths = typeof(FieldReader).GetMethods(
                            BindingFlags.Public | BindingFlags.Instance)
                            .Where(m => m.Name.StartsWith("Read")
                            && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType.Equals(typeof(string)))
                            .ToList();
                        _namedReaderMethods = meths.ToDictionary(m => m.ReturnType);
                        _miNamedReadValue = meths.Single(m => m.Name == "ReadValue");
                        _miNamedReadNullableValue = meths.Single(m => m.Name == "ReadNullableValue");
                    }
                }

                MethodInfo mi;
                _namedReaderMethods.TryGetValue(type, out mi);
                if (mi == null)
                {
                    if (TypeHelper.IsNullableType(type))
                    {
                        mi = _miNamedReadNullableValue.MakeGenericMethod(TypeHelper.GetNonNullableType(type));
                    }
                    else
                    {
                        mi = _miNamedReadValue.MakeGenericMethod(type);
                    }
                }
                return mi;
            }
        }

        protected static Dictionary<Type, MethodInfo> _ordinalReaderMethods;
        protected static Dictionary<Type, MethodInfo> _namedReaderMethods;
        protected static object _syncRoot = new object();
        protected static MethodInfo _miOrdinalReadValue;
        protected static MethodInfo _miOrdinalReadNullableValue;
        protected static MethodInfo _miNamedReadValue;
        protected static MethodInfo _miNamedReadNullableValue;
    }
}
