using SHS.Sage.Linq.Expressions.Queryable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Queryable
{
    public class RowNumberExpression : DbExpression
    {
        ReadOnlyCollection<OrderExpression> orderBy;
        public RowNumberExpression(IEnumerable<OrderExpression> orderBy)
            : base(DbExpressionType.RowCount, typeof(int))
        {
            this.orderBy = orderBy.ToReadOnly();
        }
        public ReadOnlyCollection<OrderExpression> OrderBy
        {
            get { return this.orderBy; }
        }
    }
}
