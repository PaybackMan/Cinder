using SHS.Sage.Linq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class OTrackingQueryExecutor : TrackingQueryExecutor
    {
        public OTrackingQueryExecutor(ITrackingRepository repository, QueryExecutor executor)
            : base(repository, executor)
        { }
    }
}
