using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class DataSet<T> : DataSet, IDataSet<T>
    {
        public DataSet(QueryProvider provider, IRepository repository, Expression expression)
            : base(provider, repository, typeof(T), expression)
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this._provider.Execute(this.Expression)).GetEnumerator();
        }
    }

    public class DataSet : IDataSet
    {
        public DataSet(QueryProvider provider, IRepository repository, Type identityType, Expression expression)
        {
            _provider = provider;
            if (expression == null)
            {
                this.Expression = Expression.Constant(this);
            }
            else
            {
                this.Expression = expression;
            }
            this.Repository = repository;
            this.ElementType = identityType;
        }

        public Expression Expression
        {
            get;
            private set;
        }

        protected QueryProvider _provider;
        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this._provider.Execute(this.Expression)).GetEnumerator();
        }

        public Type ElementType { get; private set; }

        public IRepository Repository
        {
            get;
            protected set;
        }
    }
}
