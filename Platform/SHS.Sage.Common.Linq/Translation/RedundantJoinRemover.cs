using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Removes joins expressions that are identical to joins that already exist
    /// </summary>
    public class RedundantJoinRemover : DbExpressionVisitor
    {
        Dictionary<IdentifiableAlias, IdentifiableAlias> map;

        protected RedundantJoinRemover()
        {
            this.map = new Dictionary<IdentifiableAlias, IdentifiableAlias>();
        }

        public static Expression Remove(Expression expression)
        {
            return new RedundantJoinRemover().Visit(expression);
        }

        protected override Expression VisitJoin(JoinExpression join)
        {
            Expression result = base.VisitJoin(join);
            join = result as JoinExpression;
            if (join != null)
            {
                AliasedExpression right = join.Right as AliasedExpression;
                if (right != null)
                {
                    AliasedExpression similarRight = (AliasedExpression)this.FindSimilarRight(join.Left as JoinExpression, join);
                    if (similarRight != null)
                    {
                        this.map.Add(right.Alias, similarRight.Alias);
                        return join.Left;
                    }
                }
            }
            return result;
        }

        private Expression FindSimilarRight(JoinExpression join, JoinExpression compareTo)
        {
            if (join == null)
                return null;
            if (join.Join == compareTo.Join)
            {
                if (join.Right.NodeType == compareTo.Right.NodeType
                    && DbExpressionComparer.AreEqual(join.Right, compareTo.Right))
                {
                    if (join.Condition == compareTo.Condition)
                        return join.Right;
                    var scope = new ScopedDictionary<IdentifiableAlias, IdentifiableAlias>(null);
                    scope.Add(((AliasedExpression)join.Right).Alias, ((AliasedExpression)compareTo.Right).Alias);
                    if (DbExpressionComparer.AreEqual(null, scope, join.Condition, compareTo.Condition))
                        return join.Right;
                }
            }
            Expression result = FindSimilarRight(join.Left as JoinExpression, compareTo);
            if (result == null)
            {
                result = FindSimilarRight(join.Right as JoinExpression, compareTo);
            }
            return result;
        }

        protected override Expression VisitField(FieldExpression field)
        {
            IdentifiableAlias mapped;
            if (this.map.TryGetValue(field.Alias, out mapped))
            {
                return new FieldExpression(field.Type, field.QueryType, mapped, field.Name);
            }
            return field;
        }
    }
}
