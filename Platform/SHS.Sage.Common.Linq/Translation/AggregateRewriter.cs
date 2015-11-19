using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Rewrite aggregate expressions, moving them into same select expression that has the group-by clause
    /// </summary>
    public class AggregateRewriter : DbExpressionVisitor
    {
        QueryLanguage language;
        ILookup<IdentifiableAlias, AggregateSubqueryExpression> lookup;
        Dictionary<AggregateSubqueryExpression, Expression> map;

        protected AggregateRewriter(QueryLanguage language, Expression expr)
        {
            this.language = language;
            this.map = new Dictionary<AggregateSubqueryExpression, Expression>();
            this.lookup = Gather(expr).ToLookup(a => a.GroupByAlias);
        }

        protected virtual List<AggregateSubqueryExpression> Gather(Expression expr)
        {
            return AggregateGatherer.Gather(expr);
        }

        public static Expression Rewrite(QueryLanguage language, Expression expr)
        {
            return new AggregateRewriter(language, expr).Visit(expr);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);
            if (lookup.Contains(select.Alias))
            {
                List<FieldDeclaration> aggFields = new List<FieldDeclaration>(select.Fields);
                foreach (AggregateSubqueryExpression ae in lookup[select.Alias])
                {
                    string name = "agg" + aggFields.Count;
                    var colType = this.language.TypeSystem.GetStorageType(ae.Type);
                    FieldDeclaration cd = new FieldDeclaration(name, ae.AggregateInGroupSelect, colType);
                    this.map.Add(ae, new FieldExpression(ae.Type, colType, ae.GroupByAlias, name));
                    aggFields.Add(cd);
                }
                return new SelectExpression(select.Alias, aggFields, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.IsReverse);
            }
            return select;
        }

        protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
        {
            Expression mapped;
            if (this.map.TryGetValue(aggregate, out mapped))
            {
                return mapped;
            }
            return this.Visit(aggregate.AggregateAsSubquery);
        }

        protected class AggregateGatherer : DbExpressionVisitor
        {
            protected List<AggregateSubqueryExpression> aggregates = new List<AggregateSubqueryExpression>();
            protected AggregateGatherer()
            {
            }

            internal static List<AggregateSubqueryExpression> Gather(Expression expression)
            {
                AggregateGatherer gatherer = new AggregateGatherer();
                gatherer.Visit(expression);
                return gatherer.aggregates;
            }

            protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
            {
                this.aggregates.Add(aggregate);
                return base.VisitAggregateSubquery(aggregate);
            }
        }
    }
}
