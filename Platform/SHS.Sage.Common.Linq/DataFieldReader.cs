using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class DataFieldReader : FieldReader
    {
        QueryExecutor _executor;
        DbDataReader _reader;

        public DataFieldReader(QueryExecutor executor, DbDataReader reader)
        {
            this._executor = executor;
            this._reader = reader;
            this.Init();
        }

        protected override int GetOrdinal(string name)
        {
            return this._reader.GetOrdinal(name);
        }

        protected override int FieldCount
        {
            get { return this._reader.FieldCount; }
        }

        protected override Type GetFieldType(int ordinal)
        {
            return this._reader.GetFieldType(ordinal);
        }

        protected override bool IsDBNull(int ordinal)
        {
            return this._reader.IsDBNull(ordinal);
        }

        protected override T GetValue<T>(int ordinal)
        {
            return (T)this._executor.Convert(this._reader.GetValue(ordinal), typeof(T));
        }

        protected override Byte GetByte(int ordinal)
        {
            return this._reader.GetByte(ordinal);
        }

        protected override Char GetChar(int ordinal)
        {
            return this._reader.GetChar(ordinal);
        }

        protected override DateTime GetDateTime(int ordinal)
        {
            return this._reader.GetDateTime(ordinal);
        }

        protected override Decimal GetDecimal(int ordinal)
        {
            return this._reader.GetDecimal(ordinal);
        }

        protected override Double GetDouble(int ordinal)
        {
            return this._reader.GetDouble(ordinal);
        }

        protected override Single GetSingle(int ordinal)
        {
            return this._reader.GetFloat(ordinal);
        }

        protected override Guid GetGuid(int ordinal)
        {
            return this._reader.GetGuid(ordinal);
        }

        protected override Int16 GetInt16(int ordinal)
        {
            return this._reader.GetInt16(ordinal);
        }

        protected override Int32 GetInt32(int ordinal)
        {
            return this._reader.GetInt32(ordinal);
        }

        protected override Int64 GetInt64(int ordinal)
        {
            return this._reader.GetInt64(ordinal);
        }

        protected override String GetString(int ordinal)
        {
            return this._reader.GetString(ordinal);
        }

        protected override Type GetFieldType(string name)
        {
            return this._reader.GetFieldType(this._reader.GetOrdinal(name));
        }

        protected override bool IsDBNull(string name)
        {
            return this._reader.IsDBNull(this._reader.GetOrdinal(name));
        }

        protected override T GetValue<T>(string name)
        {
            return this.GetValue<T>(this._reader.GetOrdinal(name));
        }

        protected override byte GetByte(string name)
        {
            return this._reader.GetByte(this._reader.GetOrdinal(name));
        }

        protected override char GetChar(string name)
        {
            return this._reader.GetChar(this._reader.GetOrdinal(name));
        }

        protected override DateTime GetDateTime(string name)
        {
            return this._reader.GetDateTime(this._reader.GetOrdinal(name));
        }

        protected override decimal GetDecimal(string name)
        {
            return this._reader.GetDecimal(this._reader.GetOrdinal(name));
        }

        protected override double GetDouble(string name)
        {
            return this._reader.GetDouble(this._reader.GetOrdinal(name));
        }

        protected override float GetSingle(string name)
        {
            return this._reader.GetFloat(this._reader.GetOrdinal(name));
        }

        protected override Guid GetGuid(string name)
        {
            return this._reader.GetGuid(this._reader.GetOrdinal(name));
        }

        protected override short GetInt16(string name)
        {
            return this._reader.GetInt16(this._reader.GetOrdinal(name));
        }

        protected override int GetInt32(string name)
        {
            return this._reader.GetInt32(this._reader.GetOrdinal(name));
        }

        protected override long GetInt64(string name)
        {
            return this._reader.GetInt64(this._reader.GetOrdinal(name));
        }

        protected override string GetString(string name)
        {
            return this._reader.GetString(this._reader.GetOrdinal(name));
        }
    }
}
