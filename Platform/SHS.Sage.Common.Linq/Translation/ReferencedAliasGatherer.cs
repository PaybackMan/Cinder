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
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    public class ReferencedAliasGatherer : DbExpressionVisitor
    {
        HashSet<IdentifiableAlias> aliases;

        private ReferencedAliasGatherer()
        {
            this.aliases = new HashSet<IdentifiableAlias>();
        }

        public static HashSet<IdentifiableAlias> Gather(Expression source)
        {
            var gatherer = new ReferencedAliasGatherer();
            gatherer.Visit(source);
            return gatherer.aliases;
        }

        protected override Expression VisitField(FieldExpression field)
        {
            this.aliases.Add(field.Alias);
            return field;
        }
    }
}
