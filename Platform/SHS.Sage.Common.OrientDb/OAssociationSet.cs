using SHS.Sage.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public class OAssociationSet<T> : AssociationSet<T>
        where T : IAssociation
    {
        public OAssociationSet(OTrackingQueryProvider provider, ORepository repository)
            : this(provider, repository, null, null)
        { }

        public OAssociationSet(OTrackingQueryProvider provider, ORepository repository, IThing root)
            : base(provider, repository, null)
        {
            this.Root = root;
        }

        public OAssociationSet(OTrackingQueryProvider provider, ORepository repository, Expression expression)
            : this(provider, repository, null, expression)
        { }

        public OAssociationSet(OTrackingQueryProvider provider, ORepository repository, IThing root, Expression expression)
            : base(provider, repository, null)
        {
            this.Root = root;
        }

        public IThing Root { get; private set; }

        public override IIdentifiable Get(IIdentifiable identity)
        {
            throw new NotImplementedException();
        }
    }
}
