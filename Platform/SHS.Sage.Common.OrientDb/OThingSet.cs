using SHS.Sage.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public class OThingSet<T> : ThingSet<T>
        where T : IThing
    {
        public OThingSet(OTrackingQueryProvider provider, ORepository repository)
            : base(provider, repository, null)
        { }

        public OThingSet(OTrackingQueryProvider provider, ORepository repository, Expression expression)
            : base(provider, repository, expression)
        { }

        public override IAssociationSet<U> AssociationSet<U>(T sourceThing)
        {
            return new OAssociationSet<U>((OTrackingQueryProvider)this._provider, (ORepository)this.Repository, sourceThing);
        }
    }
}
