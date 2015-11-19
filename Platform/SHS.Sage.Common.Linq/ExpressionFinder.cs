using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class ExpressionFinder<TExpression> : ExpressionVisitor
         where TExpression : Expression
    {
        private readonly Type _findType;

        private ExpressionFinder(Expression source, Type findType)
        {
            this._findType = findType;
            this.Visit(source);
        }
        public static TExpression FindFirst(Expression source) 
        {
            var finder = new ExpressionFinder<TExpression>(source, typeof(TExpression));
            return finder.Found;
        }

        protected override Expression Visit(Expression exp)
        {
            if (Found != null) return exp;

            if (exp.GetType().IsTypeOfOrSubclassOf(this._findType))
            {
                this.Found = (TExpression)exp;
            }
  
            return base.Visit(exp);
        }

        public TExpression Found { get; private set; }
    }
}
