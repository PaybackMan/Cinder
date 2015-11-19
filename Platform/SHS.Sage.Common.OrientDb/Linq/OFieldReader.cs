using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class OFieldReader : DataFieldReader
    {
        public OFieldReader(QueryExecutor executor, DbDataReader reader)
            : base(executor, reader)
        { }
    }
}
