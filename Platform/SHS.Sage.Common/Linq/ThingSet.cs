using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public abstract class ThingSet<T> : IdentifiableSet<T>, IThingSet<T>
        where T : IThing
    {
        protected ThingSet(QueryProvider provider, IRepository repository, Expression expression)
            : base(provider, repository, expression)
        {
        }

        public abstract IAssociationSet<U> AssociationSet<U>(T sourceThing) where U : IAssociation;
    }
}
