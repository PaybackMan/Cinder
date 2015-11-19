using SHS.Sage.Linq.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public class ProxiedIdentifiableCopier : IdentifiableCopier
    {
        public ProxiedIdentifiableCopier(IRepository repository) : base(repository) { }

        public override IIdentifiable Clone(IIdentifiable source)
        {
            if (!(source is IProxyIdentifiable))
            {
                source = MakeProxy(source);
            }
            var clone = base.Clone(source);
            if (!(clone is IProxyIdentifiable))
            {
                clone = MakeProxy(clone);
            }
            return clone;
        }

        private IIdentifiable MakeProxy(IIdentifiable source)
        {
            var proxy = IdentifiableProxyBuilder.CreateProxy(source.GetType(), Repository);
            Copy(source, proxy);
            return proxy;
        }
    }
}
