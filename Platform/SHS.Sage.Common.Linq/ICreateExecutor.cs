using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public interface ICreateExecutor
    {
        QueryExecutor CreateExecutor();
    }
}
