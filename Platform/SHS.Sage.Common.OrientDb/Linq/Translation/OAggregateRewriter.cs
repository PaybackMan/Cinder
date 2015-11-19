using SHS.Sage.Common.Linq.Expressions;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SHS.Sage.Linq.Expressions.Queryable;

namespace SHS.Sage.Common.Linq.Translation
{
    public class OAggregateRewriter : AggregateRewriter
    {
        protected OAggregateRewriter(QueryLanguage language, Expression expr)
            : base(language, expr)
        {

        }

        public new static Expression Rewrite(QueryLanguage language, Expression expr)
        {
            return new OAggregateRewriter(language, expr).Visit(expr);
        }

        protected override List<AggregateSubqueryExpression> Gather(Expression expr)
        {
            return OAggregateGatherer.Gather(expr);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null) return null;

            switch((ODbExpressionType)exp.NodeType)
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

        protected class OAggregateGatherer : AggregateGatherer
        {
            internal static List<AggregateSubqueryExpression> Gather(Expression expression)
            {
                OAggregateGatherer gatherer = new OAggregateGatherer();
                gatherer.Visit(expression);
                return gatherer.aggregates;
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
}
