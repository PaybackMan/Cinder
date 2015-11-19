using SHS.Sage.Common.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Common.Linq.Translation
{
    public class ORedundantSubqueryRemover : RedundantSubqueryRemover
    {
        public new static Expression Remove(Expression expression)
        {
            return new ORedundantSubqueryRemover().Visit(expression);
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

        protected override Expression VisitSelect(SelectExpression select)
        {
            var from = this.VisitSource(select.From);
            var where = this.Visit(select.Where);
            var orderBy = this.VisitOrderBy(select.OrderBy);
            var groupBy = this.VisitExpressionList(select.GroupBy);
            var skip = this.Visit(select.Skip);
            var take = this.Visit(select.Take);
            var fields = this.VisitFieldDeclarations(select.Fields);
            select = this.UpdateSelect(select, from, where, orderBy, groupBy, skip, take, select.IsDistinct, select.IsReverse, fields);

            // first remove all purely redundant subqueries
            List<SelectExpression> redundant = ORedundantSubqueryGatherer.Gather(select.From);
            if (redundant != null)
            {
                select = OSubqueryRemover.Remove(select, redundant);
            }

            return select;
        }

        protected class ORedundantSubqueryGatherer : RedundantSubqueryGatherer
        {
            protected ORedundantSubqueryGatherer():base()
            {
            }

            internal static List<SelectExpression> Gather(Expression source)
            {
                var gatherer = new ORedundantSubqueryGatherer();
                gatherer.Visit(source);
                return gatherer.redundant;
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

