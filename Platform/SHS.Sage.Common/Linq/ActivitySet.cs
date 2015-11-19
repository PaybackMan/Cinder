using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public abstract class ActivitySet<T> : IdentifiableSet<T>, IActivitySet<T>
       where T : IActivity
    {
        protected ActivitySet(QueryProvider provider, IRepository repository, Expression expression)
            : base(provider, repository, expression)
        {
        }
    }
}
