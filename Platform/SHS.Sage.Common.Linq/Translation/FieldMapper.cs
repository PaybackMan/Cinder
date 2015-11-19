using SHS.Sage.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Rewrite all field references to one or more aliases to a new single alias
    /// </summary>
    public class FieldMapper : DbExpressionVisitor
    {
        HashSet<IdentifiableAlias> oldAliases;
        IdentifiableAlias newAlias;

        private FieldMapper(IEnumerable<IdentifiableAlias> oldAliases, IdentifiableAlias newAlias)
        {
            this.oldAliases = new HashSet<IdentifiableAlias>(oldAliases);
            this.newAlias = newAlias;
        }

        public static Expression Map(Expression expression, IdentifiableAlias newAlias, IEnumerable<IdentifiableAlias> oldAliases)
        {
            return new FieldMapper(oldAliases, newAlias).Visit(expression);
        }

        public static Expression Map(Expression expression, IdentifiableAlias newAlias, params IdentifiableAlias[] oldAliases)
        {
            return Map(expression, newAlias, (IEnumerable<IdentifiableAlias>)oldAliases);
        }

        protected override Expression VisitField(FieldExpression field)
        {
            if (this.oldAliases.Contains(field.Alias))
            {
                return new FieldExpression(field.Type, field.QueryType, this.newAlias, field.Name);
            }
            return field;
        }
    }
}
