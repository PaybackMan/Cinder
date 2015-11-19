using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public abstract class AliasedExpression : DbExpression
    {
        IdentifiableAlias alias;
        protected AliasedExpression(DbExpressionType nodeType, Type type, IdentifiableAlias alias)
            : base(nodeType, type)
        {
            this.alias = alias;
        }
        public IdentifiableAlias Alias
        {
            get { return this.alias; }
        }
    }
}
