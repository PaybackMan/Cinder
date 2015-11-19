using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public class BetweenExpression : DbExpression
    {
        Expression expression;
        Expression lower;
        Expression upper;
        public BetweenExpression(Expression expression, Expression lower, Expression upper)
            : base(DbExpressionType.Between, expression.Type)
        {
            this.expression = expression;
            this.lower = lower;
            this.upper = upper;
        }
        public Expression Expression
        {
            get { return this.expression; }
        }
        public Expression Lower
        {
            get { return this.lower; }
        }
        public Expression Upper
        {
            get { return this.upper; }
        }
    }
}
