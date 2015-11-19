using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Queryable
{
    public class InExpression : SubqueryExpression
    {
        Expression expression;
        ReadOnlyCollection<Expression> values;  // either select or expressions are assigned
        public InExpression(Expression expression, SelectExpression select)
            : base(DbExpressionType.In, typeof(bool), select)
        {
            this.expression = expression;
        }
        public InExpression(Expression expression, IEnumerable<Expression> values)
            : base(DbExpressionType.In, typeof(bool), null)
        {
            this.expression = expression;
            this.values = values.ToReadOnly();
        }
        public Expression Expression
        {
            get { return this.expression; }
        }
        public ReadOnlyCollection<Expression> Values
        {
            get { return this.values; }
        }
    }
}
