using SHS.Sage.Linq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class OTrackingQueryProvider : TrackingQueryProvider
    {
        public OTrackingQueryProvider(ORepository respository)
            : base(new OQueryProvider(respository))
        {
            this.Cache = respository.QueryCache;
        }

        protected override QueryExecutor CreateExecutor()
        {
            return new OTrackingQueryExecutor((ITrackingRepository)this.Repository, ((ICreateExecutor)this.Provider).CreateExecutor());
        }
    }
}
