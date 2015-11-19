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
    /// Duplicate the query expression by making a copy with new table aliases
    /// </summary>
    public class QueryDuplicator : DbExpressionVisitor
    {
        Dictionary<IdentifiableAlias, IdentifiableAlias> map = new Dictionary<IdentifiableAlias, IdentifiableAlias>();

        public static Expression Duplicate(Expression expression)
        {
            return new QueryDuplicator().Visit(expression);
        }

        protected override Expression VisitIdentifiable(IdentifiableExpression table)
        {
            IdentifiableAlias newAlias = new IdentifiableAlias();
            this.map[table.Alias] = newAlias;
            return new IdentifiableExpression(newAlias, table.Entity, table.Name);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            IdentifiableAlias newAlias = new IdentifiableAlias();
            this.map[select.Alias] = newAlias;
            select = (SelectExpression)base.VisitSelect(select);
            return new SelectExpression(newAlias, select.Fields, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.IsReverse);
        }

        protected override Expression VisitField(FieldExpression field)
        {
            IdentifiableAlias newAlias;
            if (this.map.TryGetValue(field.Alias, out newAlias))
            {
                return new FieldExpression(field.Type, field.QueryType, newAlias, field.Name);
            }
            return field;
        }
    }
}
