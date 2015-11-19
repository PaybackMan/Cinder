using SHS.Sage.Common.Linq.Language;
using SHS.Sage.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Common.Linq.Expressions
{
    public enum AssociationOrigin
    {
        Source,
        Target
    }
    public class OAssociationFilterExpression : AliasedExpression
    {
        public OAssociationFilterExpression(IdentifiableAlias alias, AssociationOrigin origin, Type type, Expression source, Expression predicate)
            : base((DbExpressionType)ODbExpressionType.AssociationFilter, type, alias)
        {
            Source = source;
            Predicate = predicate;
            Origin = origin;
        }

        public Expression Source { get; private set; }
        public Expression Predicate { get; private set; }
        public AssociationOrigin Origin { get; private set; }

        public override string ToString()
        {
            return OQueryFormatter.Format(this, true);
        }
    }
}
