using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public interface IProvideRepository : IQueryProvider
    {
        IRepository Repository { get; }
        bool CanEvaluateLocally(Expression expresison);
        bool CanBeParameter(Expression expression);
    }
}
