using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public abstract class QueryProvider : IQueryProvider, IProvideQueryText
    {
        public IQueryable<TIdentity> CreateQuery<TIdentity>(Expression expression)
        {
            return (IQueryable<TIdentity>)CreateQuery(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        public abstract IQueryable CreateQuery(Expression expression);
        public abstract object Execute(Expression expression);
        public abstract string ToString(Expression queryExpression);
    }
}
