using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class IfCommandExpression : CommandExpression
    {
        Expression check;
        Expression ifTrue;
        Expression ifFalse;

        public IfCommandExpression(Expression check, Expression ifTrue, Expression ifFalse)
            : base(DbExpressionType.If, ifTrue.Type)
        {
            this.check = check;
            this.ifTrue = ifTrue;
            this.ifFalse = ifFalse;
        }

        public Expression Check
        {
            get { return this.check; }
        }

        public Expression IfTrue
        {
            get { return this.ifTrue; }
        }

        public Expression IfFalse
        {
            get { return this.ifFalse; }
        }
    }
}
