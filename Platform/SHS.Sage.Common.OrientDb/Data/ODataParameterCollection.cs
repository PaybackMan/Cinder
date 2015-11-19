using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Data
{
	public class ODataParameterCollection : DbParameterCollection
    {
        List<ODataParameter> _list = new List<ODataParameter>();
        public override int Add(object value)
        {
            _list.Add((ODataParameter)value);
            return 1;
        }

        public override void AddRange(Array values)
        {
            _list.AddRange(values.OfType<ODataParameter>());
        }

        public override void Clear()
        {
            _list.Clear();
        }

        public override bool Contains(string value)
        {
            return _list.Any(p => p.ParameterName.Equals(value));
        }

        public override bool Contains(object value)
        {
            return _list.Contains(value as ODataParameter);
        }

        public override void CopyTo(Array array, int index)
        {
            _list.CopyTo(array.OfType<ODataParameter>().ToArray(), index);
        }

        public override int Count
        {
            get { return _list.Count; }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return _list.Single(p => p.ParameterName.Equals(parameterName));
        }

        protected override DbParameter GetParameter(int index)
        {
            return _list[index];
        }

        public override int IndexOf(string parameterName)
        {
            return _list.IndexOf((ODataParameter)GetParameter(parameterName));
        }

        public override int IndexOf(object value)
        {
            return IndexOf(((DbParameter)value).ParameterName);
        }

        public override void Insert(int index, object value)
        {
            _list.Insert(index, (ODataParameter)value);
        }

        public override void Remove(object value)
        {
            _list.Remove((ODataParameter)value);
        }

        public override void RemoveAt(string parameterName)
        {
            _list.Remove((ODataParameter)GetParameter(parameterName));
        }

        public override void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            _list[IndexOf(parameterName)] = (ODataParameter)value;
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _list[index] = (ODataParameter)value;
        }

        public override object SyncRoot
        {
            get 
            {
                return _list;
            }
        }

		#region implemented abstract members of DbParameterCollection

		public override bool IsFixedSize {
			get {
				return false;
			}
		}

		public override bool IsReadOnly {
			get {
				return false;
			}
		}

		public override bool IsSynchronized {
			get {
				return false;
			}
		}

		#endregion
    }
}
