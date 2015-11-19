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
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    public class DeclaredAliasGatherer : DbExpressionVisitor
    {
        HashSet<IdentifiableAlias> aliases;

        private DeclaredAliasGatherer()
        {
            this.aliases = new HashSet<IdentifiableAlias>();
        }

        public static HashSet<IdentifiableAlias> Gather(Expression source)
        {
            var gatherer = new DeclaredAliasGatherer();
            gatherer.Visit(source);
            return gatherer.aliases;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            this.aliases.Add(select.Alias);
            return select;
        }

        protected override Expression VisitIdentifiable(IdentifiableExpression table)
        {
            this.aliases.Add(table.Alias);
            return table;
        }
    }
}
