using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Removes field declarations in SelectExpression's that are not referenced
    /// </summary>
    public class UnusedFieldRemover : DbExpressionVisitor
    {
        Dictionary<IdentifiableAlias, HashSet<string>> allFieldsUsed;
        bool retainAllFields;

        protected UnusedFieldRemover()
        {
            this.allFieldsUsed = new Dictionary<IdentifiableAlias, HashSet<string>>();
        }

        public static Expression Remove(Expression expression)
        {
            return new UnusedFieldRemover().Visit(expression);
        }

        private void MarkFieldAsUsed(IdentifiableAlias alias, string name)
        {
            HashSet<string> fields;
            if (!this.allFieldsUsed.TryGetValue(alias, out fields))
            {
                fields = new HashSet<string>();
                this.allFieldsUsed.Add(alias, fields);
            }
            fields.Add(name);
        }

        private bool IsFieldUsed(IdentifiableAlias alias, string name)
        {
            HashSet<string> fieldsUsed;
            if (this.allFieldsUsed.TryGetValue(alias, out fieldsUsed))
            {
                if (fieldsUsed != null)
                {
                    return fieldsUsed.Contains(name);
                }
            }
            return false;
        }

        private void ClearFieldsUsed(IdentifiableAlias alias)
        {
            this.allFieldsUsed[alias] = new HashSet<string>();
        }

        protected override Expression VisitField(FieldExpression field)
        {
            MarkFieldAsUsed(field.Alias, field.Name);
            return field;
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery)
        {
            if ((subquery.NodeType == (ExpressionType)DbExpressionType.Scalar ||
                subquery.NodeType == (ExpressionType)DbExpressionType.In) &&
                subquery.Select != null)
            {
                System.Diagnostics.Debug.Assert(subquery.Select.Fields.Count == 1);
                MarkFieldAsUsed(subquery.Select.Alias, subquery.Select.Fields[0].Name);
            }
            return base.VisitSubquery(subquery);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            // visit field projection first
            ReadOnlyCollection<FieldDeclaration> fields = select.Fields;

            var wasRetained = this.retainAllFields;
            this.retainAllFields = false;

            List<FieldDeclaration> alternate = null;
            for (int i = 0, n = select.Fields.Count; i < n; i++)
            {
                FieldDeclaration decl = select.Fields[i];
                if (wasRetained || select.IsDistinct || IsFieldUsed(select.Alias, decl.Name))
                {
                    Expression expr = this.Visit(decl.Expression);
                    if (expr != decl.Expression)
                    {
                        decl = new FieldDeclaration(decl.Name, expr, decl.QueryType);
                    }
                }
                else
                {
                    decl = null;  // null means it gets omitted
                }
                if (decl != select.Fields[i] && alternate == null)
                {
                    alternate = new List<FieldDeclaration>();
                    for (int j = 0; j < i; j++)
                    {
                        alternate.Add(select.Fields[j]);
                    }
                }
                if (decl != null && alternate != null)
                {
                    alternate.Add(decl);
                }
            }
            if (alternate != null)
            {
                fields = alternate.AsReadOnly();
            }

            Expression take = this.Visit(select.Take);
            Expression skip = this.Visit(select.Skip);
            ReadOnlyCollection<Expression> groupbys = this.VisitExpressionList(select.GroupBy);
            ReadOnlyCollection<OrderExpression> orderbys = this.VisitOrderBy(select.OrderBy);
            Expression where = this.Visit(select.Where);

            Expression from = this.Visit(select.From);

            ClearFieldsUsed(select.Alias);

            if (fields != select.Fields
                || take != select.Take
                || skip != select.Skip
                || orderbys != select.OrderBy
                || groupbys != select.GroupBy
                || where != select.Where
                || from != select.From)
            {
                select = new SelectExpression(select.Alias, fields, from, where, orderbys, groupbys, select.IsDistinct, skip, take, select.IsReverse);
            }

            this.retainAllFields = wasRetained;

            return select;
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate)
        {
            // COUNT(*) forces all fields to be retained in subquery
            if (aggregate.AggregateName == "Count" && aggregate.Argument == null)
            {
                this.retainAllFields = true;
            }
            return base.VisitAggregate(aggregate);
        }

        protected override Expression VisitProjection(ProjectionExpression projection)
        {
            // visit mapping in reverse order
            Expression projector = this.Visit(projection.Projector);
            SelectExpression select = (SelectExpression)this.Visit(projection.Select);
            return this.UpdateProjection(projection, select, projector, projection.Aggregator);
        }

        protected override Expression VisitClientJoin(ClientJoinExpression join)
        {
            var innerKey = this.VisitExpressionList(join.InnerKey);
            var outerKey = this.VisitExpressionList(join.OuterKey);
            ProjectionExpression projection = (ProjectionExpression)this.Visit(join.Projection);
            if (projection != join.Projection || innerKey != join.InnerKey || outerKey != join.OuterKey)
            {
                return new ClientJoinExpression(projection, outerKey, innerKey);
            }
            return join;
        }

        protected override Expression VisitJoin(JoinExpression join)
        {
            if (join.Join == JoinType.SingletonLeftOuter)
            {
                // first visit right side w/o looking at condition
                Expression right = this.Visit(join.Right);
                AliasedExpression ax = right as AliasedExpression;
                if (ax != null && !this.allFieldsUsed.ContainsKey(ax.Alias))
                {
                    // if nothing references the alias on the right, then the join is redundant
                    return this.Visit(join.Left);
                }
                // otherwise do it the right way
                Expression cond = this.Visit(join.Condition);
                Expression left = this.Visit(join.Left);
                right = this.Visit(join.Right);
                return this.UpdateJoin(join, join.Join, left, right, cond);
            }
            else
            {
                // visit join in reverse order
                Expression condition = this.Visit(join.Condition);
                Expression right = this.VisitSource(join.Right);
                Expression left = this.VisitSource(join.Left);
                return this.UpdateJoin(join, join.Join, left, right, condition);
            }
        }
    }
}
