using Orient.Client;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;
using SHS.Sage.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHS.Sage.Data
{
	internal class OResponseReader : IEnumerable<IDataRecord>, IDisposable
    {
        Enumerator _en;
        Response _response;
        PayloadStatus _status;
        OperationMode _mode;
        IDataReader _reader;

        public OResponseReader(IDataReader reader, Response response, OperationMode mode)
        {
            _response = response;
            _status = (PayloadStatus)_response.Reader.ReadByte();
            _mode = mode;
            _reader = reader;
            HasRows = _status != PayloadStatus.NoRemainingRecords;
            RecordsAffected = _status != PayloadStatus.NullResult && _status != PayloadStatus.NullResult ? 1 : 0;
        }

        public IEnumerator<IDataRecord> GetEnumerator()
        {
            if (_en == null)
            {
                _en = new Enumerator(_reader, _response, _status, _mode);
            }
            return _en;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool HasRows
        {
            get;
            private set;
        }

        public int RecordsAffected
        {
            get;
            private set;
        }

		#region IDisposable implementation

		public void Dispose ()
		{
			if (_en != null) {
				_en.Dispose ();
				_en = null;
			}
		}

		#endregion

        class Enumerator : IEnumerator<IDataRecord>
        {
            Response _response;
            PayloadStatus _status;
            OperationMode _mode;
            IDataReader _reader;
            public Enumerator(IDataReader reader, Response response, PayloadStatus status, OperationMode mode)
            {
                _response = response;
                _status = status;
                _mode = mode;
                _reader = reader;
            }

            public IDataRecord Current
            {
                get;
                private set;
            }

            public void Dispose()
            {

            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                var canMove = this._status != PayloadStatus.NoRemainingRecords;
                if (canMove)
                {
                    this.Current = CreateRecord();
                }
                return canMove;
            }

            private IDataRecord CreateRecord()
            {
                if (_mode == OperationMode.Asynchronous)
                    return CreateRecordFromAsync();
                else
                    return CreateRecordFromSync();
            }

            private IDataRecord CreateRecordFromAsync()
            {
                var record = CreateRecordFromStream();
                this._status = (PayloadStatus)this._response.Reader.ReadByte();
                return record;
            }

            int _recordIndex = 0;
            int _recordCount = 0;
            private IDataRecord CreateRecordFromSync()
            {
                int contentLength;
                IDataRecord record = null;
                var reader = _response.Reader;

                switch (_status)
                {
                    case PayloadStatus.NullResult: // 'n'
                        // nothing to do
                        break;
                    case PayloadStatus.SingleRecord: // 'r'
                        record = CreateRecordFromStream();
                        _status = PayloadStatus.NoRemainingRecords;
                        break;
                    case PayloadStatus.SerializedResult: // 'a'
                        // TODO: how to parse result - string?
                        contentLength = reader.ReadInt32EndianAware();
                        string serialized = System.Text.Encoding.Default.GetString(reader.ReadBytes(contentLength));
                        record = new ODataRecord(_reader, new []{ new KeyValuePair<string, object>("Value", serialized) });
                        _status = PayloadStatus.NoRemainingRecords;
                        break;
                    case PayloadStatus.RecordCollection: // 'l'
                        if (_recordIndex == 0)
                        {
                            _recordCount = reader.ReadInt32EndianAware();
                        }

                        if (_recordIndex < _recordCount)
                        {
                            record = CreateRecordFromStream();
                            _recordIndex++;
                        }

                        _status = _recordCount > _recordIndex ? _status : PayloadStatus.NoRemainingRecords;

                        break;
                    default:
                        break;
                }

                //if (OClient.ProtocolVersion >= 17)
                //{
                //    //Load the fetched records in cache
                //    //while ((payloadStatus = (PayloadStatus)reader.ReadByte()) != PayloadStatus.NoRemainingRecords)
                //    //{
                //    //    ODocument document = ParseDocument(reader);
                //    //    if (document != null && payloadStatus == PayloadStatus.PreFetched)
                //    //    {
                //    //        //Put in the client local cache
                //    //        response.Connection.Database.ClientCache[document.ORID] = document;
                //    //    }
                //    //}
                //}

                return record;
            }

            private IDataRecord CreateRecordFromStream()
            {
                var reader = _response.Reader;
                short classId = reader.ReadInt16EndianAware();
                ORID id = null;
                bool hasBody = true;
                ORecordType type = ORecordType.Document;

                if (classId == -2)
                {
                    return null;
                }
                else
                {
                    if (classId == -3)
                        hasBody = false;
                    else
                        type = (ORecordType)reader.ReadByte();

                    id = new ORID();
                    id.ClusterId = reader.ReadInt16EndianAware();
                    id.ClusterPosition = reader.ReadInt64EndianAware();
                }

                if (hasBody)
                {
                    int version = reader.ReadInt32EndianAware();
                    int recordLength = reader.ReadInt32EndianAware();
                    byte[] serializedRecord = reader.ReadBytes(recordLength);

                    return new ODataRecord(this._reader, id, version, type, classId, serializedRecord);
                }
                else
                {
                    return new ODataRecord(this._reader, id, 0, type, classId, new byte[0]);
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }


        class ODataRecord : IDataRecord
        {


            static Dictionary<Type, Delegate> _converters = new Dictionary<Type, Delegate>();
            static ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

            IEnumerable<KeyValuePair<string, object>> _fields;
            IDataReader _reader;
            byte[] _serializedRecord;
            public ODataRecord(IDataReader reader, ORID id, int version, ORecordType type, short classId, byte[] serializedRecord)
            {
                _reader = reader;
                _serializedRecord = serializedRecord;
                Id = id;
                DeserializeRecord();
            }

            public ODataRecord(IDataReader reader, IEnumerable<KeyValuePair<string, object>> fields)
            {
                _fields = fields;
                _reader = reader;
            }

            private void DeserializeRecord()
            {
                _fields = new ORecordDeserializer().Deserialize(_serializedRecord);
                if (!_fields.Any(kvp => kvp.Key.Equals("Id")))
                {
                    var newFields = new List<KeyValuePair<string, object>>();
                    newFields.Add(new KeyValuePair<string, object>("Id", this.Id.ToString()));
                    newFields.AddRange(_fields);
                    _fields = newFields.ToReadOnly();
                }
                else
                {
                    var kvp = _fields.First(f => f.Key.Equals("Id"));

                    if (kvp.Value == null)
                    {
                        kvp = new KeyValuePair<string, object>("Id", this.Id.ToString());
                        var newFields = new List<KeyValuePair<string, object>>();
                        newFields.Add(kvp);
                        newFields.AddRange(_fields.Where(e => e.Key != "Id"));
                        _fields = newFields.ToReadOnly();
                    }
                    else
                    {
                        this.Id = new ORID(kvp.Value.ToString());
                    }
                }
            }

            public ORID Id { get; private set; }

            public int FieldCount { get { return _fields.Count(); } }

            public bool GetBoolean(int i)
            {
                return GetValue<bool>(i);
            }

            public byte GetByte(int i)
            {
                return GetValue<byte>(i);
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                var byteArray = GetValue<byte[]>(i);
                var read = 0l;
                for (long x = fieldOffset; read < length; x++)
                {
                    buffer[bufferoffset + x - fieldOffset] = byteArray[x];
                    read++;
                }
                return read;
            }

            public char GetChar(int i)
            {
                return GetValue<char>(i);
            }

            public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
            {
                var charArray = GetValue<char[]>(i);
                var read = 0l;
                for (long x = fieldOffset; read < length; x++)
                {
                    buffer[bufferoffset + x - fieldOffset] = charArray[x];
                    read++;
                }
                return read;
            }

            public IDataReader GetData(int i)
            {
                return _reader;
            }

            public string GetDataTypeName(int i)
            {
                return this[i].GetType().Name;
            }

            public DateTime GetDateTime(int i)
            {
                return GetValue<DateTime>(i);
            }

            public decimal GetDecimal(int i)
            {
                return GetValue<decimal>(i);
            }

            public double GetDouble(int i)
            {
                return GetValue<double>(i);
            }

            public Type GetFieldType(int i)
            {
                return (this[i] ?? new object()).GetType();
            }

            public float GetFloat(int i)
            {
                return GetValue<float>(i);
            }

            public Guid GetGuid(int i)
            {
                return GetValue<Guid>(i);
            }

            public short GetInt16(int i)
            {
                return GetValue<short>(i);
            }

            public int GetInt32(int i)
            {
                return GetValue<int>(i);
            }

            public long GetInt64(int i)
            {
                return GetValue<long>(i);
            }

            public string GetName(int i)
            {
                try
                {
                    return _fields.Skip(i)
                            .Take(1)
                            .Single()
                            .Key;
                }
                catch { return null; }
            }

            public int GetOrdinal(string name)
            {
                try
                {
                    return _fields.Select((value, index) => new { value, index })
                            .Where(pair => pair.value.Key == name)
                            .Select(pair => pair.index + 1)
                            .First() - 1;
                }
                catch { return -1; }
            }

            public string GetString(int i)
            {
                var value = GetOrdinalValue(i);
                return value == null ? null : value.ToString();
            }

            public object GetValue(int i)
            {
                return GetOrdinalValue(i);
            }

            public int GetValues(object[] values)
            {
                var least = Math.Min(values.Length, _fields.Count());
                for (int i = 0; i < least; i++)
                {
                    values[i] = GetOrdinalValue(i);
                }
                return least;
            }

            public bool IsDBNull(int i)
            {
                return DBNull.Value.Equals(this[i]);
            }

            public object this[string name]
            {
                get { return GetNamedValue(name); }
            }

            public object this[int i]
            {
                get { return GetOrdinalValue(i); }
            }

            protected object GetOrdinalValue(int i)
            {
                if (i >= 0 && i < _fields.Count())
                    return _fields.ElementAt(i).Value;
                else return null;
            }

            protected object GetNamedValue(string name)
            {
                var found = _fields.SingleOrDefault(kvp => kvp.Key.Equals(name));
                if (string.IsNullOrEmpty(found.Key))
                {
                    return null;
                }
                else return found.Value;
            }

            protected T GetValue<T>(int ordinal)
            {
                Delegate converter;
                var objValue = this[ordinal];
                var targetType = typeof(T);
                _rwLock.EnterUpgradeableReadLock();
                try
                {
                    if (!_converters.TryGetValue(targetType, out converter))
                    {
                        var parm = Expression.Parameter(typeof(object));
                        var cast = Expression.Convert(Expression.Convert(Expression.Convert(parm, objValue.GetType()), targetType), typeof(object));
                        converter = Expression.Lambda<Func<object, object>>(cast, parm).Compile();
                        _rwLock.EnterWriteLock();
                        try
                        {
                            _converters.Add(targetType, converter);
                        }
                        finally
                        {
                            _rwLock.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _rwLock.ExitUpgradeableReadLock();
                }
                return (T)converter.DynamicInvoke(objValue);
            }
        }
    }
}
