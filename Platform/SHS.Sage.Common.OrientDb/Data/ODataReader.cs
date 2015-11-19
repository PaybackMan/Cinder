using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHS.Sage.Data
{
	public class ODataReader : DbDataReader, IReadData
    {
        Response _response;
        OResponseReader _reader;
        IEnumerator<IDataRecord> _enumerator;
        IOperation _operation;
        ODataCommand _command;

        internal ODataReader(ODataCommand command)
        {
            _command = command;
        }

        private IOperation CreateOperation()
        {
            return _command.CreateOperation();
        }

        /// <summary>
        /// For nested sub-queries, returns the nesting depth.  For Orient queries, this is always 0.
        /// </summary>
        public override int Depth
        {
            get { return 0; }
        }

        /// <summary>
        /// Returns the number of fields in the current row
        /// </summary>
        public override int FieldCount
        {
            get { return _enumerator == null ? -1 : _enumerator.Current == null ? -1 :  _enumerator.Current.FieldCount; }
        }

        public override bool GetBoolean(int ordinal)
        {
            return _enumerator.Current.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return _enumerator.Current.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return _enumerator.Current.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            return _enumerator.Current.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return _enumerator.Current.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return _enumerator.Current.GetDataTypeName(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return _enumerator.Current.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return _enumerator.Current.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return _enumerator.Current.GetDouble(ordinal);
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return this._enumerator;
        }

        public override Type GetFieldType(int ordinal)
        {
            return _enumerator.Current.GetFieldType(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return _enumerator.Current.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return _enumerator.Current.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return _enumerator.Current.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return _enumerator.Current.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return _enumerator.Current.GetInt64(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return _enumerator.Current.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return _enumerator.Current.GetOrdinal(name);
        }

        public override string GetString(int ordinal)
        {
            return _enumerator.Current.GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return _enumerator.Current[ordinal];
        }

        public override int GetValues(object[] values)
        {
            return _enumerator.Current.GetValues(values);
        }

        public override bool HasRows
        {
            get { return _reader.HasRows; }
        }

        public override bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsDBNull(int ordinal)
        {
            return _enumerator.Current.IsDBNull(ordinal);
        }

        public override bool NextResult()
        {
            return false;
        }

        bool _isExecuted = false;
        public override bool Read()
        {
            if (!_isExecuted)
            {
                _isExecuted = true;
                _operation = CreateOperation();
                _command.Database.Connection.ExecuteOperation(_operation, r => _response = r);
                _reader = new OResponseReader(this, _response, _command.IsIdempotent ? OperationMode.Asynchronous : OperationMode.Synchronous);
                _enumerator = _reader.GetEnumerator();
            }
            return _enumerator.MoveNext();
        }

        public override int RecordsAffected
        {
            get
            {
                return _reader.RecordsAffected;
            }
        }

        public override object this[string name]
        {
            get { return _enumerator.Current[name]; }
        }

        public override object this[int ordinal]
        {
            get { return _enumerator.Current[ordinal]; }
        }

		#region implemented abstract members of DbDataReader

		public override void Close ()
		{
			if (_reader != null) {
				_reader.Dispose ();
				_reader = null;
			}
		}

		public override DataTable GetSchemaTable ()
		{
			throw new NotSupportedException ();
		}

		#endregion
    }
}
