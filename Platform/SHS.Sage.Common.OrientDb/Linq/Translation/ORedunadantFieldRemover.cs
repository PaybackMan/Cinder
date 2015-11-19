using SHS.Sage.Common.Linq.Expressions;
using SHS.Sage.Linq.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Common.Linq.Translation
{
    public class ORedundantFieldRemover : RedundantFieldRemover
    {
        public new static Expression Remove(Expression expression)
        {
            return new ORedundantFieldRemover().Visit(expression);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null) return null;

            switch ((ODbExpressionType)exp.NodeType)
            {
                case ODbExpressionType.AssociationFilter:
                    return this.VisitAssociationFilter((OAssociationFilterExpression)exp);
                case ODbExpressionType.AssociationFilterTerminal:
                    return this.VisitAssociationFilterTerminal((OAssociationFilterTerminalExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }

        private Expression VisitAssociationFilterTerminal(OAssociationFilterTerminalExpression exp)
        {
            return exp;
        }

        private Expression VisitAssociationFilter(OAssociationFilterExpression exp)
        {
            return exp;
        }
    }
}
