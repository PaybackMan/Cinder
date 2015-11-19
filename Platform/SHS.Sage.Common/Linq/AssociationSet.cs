using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public abstract class AssociationSet<T> : IdentifiableSet<T>, IAssociationSet<T>
        where T : IAssociation
    {
        protected AssociationSet(QueryProvider provider, IRepository repository, Expression expression)
            : base(provider, repository, expression)
        {
        }
    }
}
