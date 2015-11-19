using SHS.Sage.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public class OActivitySet<T> : ActivitySet<T>
        where T : IActivity
    {
        public OActivitySet(OTrackingQueryProvider provider, ORepository repository)
            : base(provider, repository, null)
        { }

        public OActivitySet(OTrackingQueryProvider provider, ORepository repository, Expression expression)
           : base(provider, repository, expression)
        { }

        public override IIdentifiable Get(IIdentifiable identity)
        {
            throw new NotImplementedException();
        }
    }
}
