using SHS.Sage.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Common.Linq
{
    public enum ODbExpressionType : int
    {
        AssociationFilter = 10000,
        AssociationFilterTerminal,
        FilterEqual
    }
}
