using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Queryable
{
    public class AggregateExpression : DbExpression
    {
        string aggregateName;
        Expression argument;
        bool isDistinct;
        public AggregateExpression(Type type, string aggregateName, Expression argument, bool isDistinct)
            : base(DbExpressionType.Aggregate, type)
        {
            this.aggregateName = aggregateName;
            this.argument = argument;
            this.isDistinct = isDistinct;
        }
        public string AggregateName
        {
            get { return this.aggregateName; }
        }
        public Expression Argument
        {
            get { return this.argument; }
        }
        public bool IsDistinct
        {
            get { return this.isDistinct; }
        }
    }
}
