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
    public enum FilterTerminalType
    {
        Count
    }
    public class OAssociationFilterTerminalExpression : DbExpression
    {
        public OAssociationFilterTerminalExpression(FilterTerminalType terminalType, Type type, Expression filter, Expression predicate)
            : base((DbExpressionType)ODbExpressionType.AssociationFilterTerminal, type)
        {
            this.FilterTerminalType = terminalType;
            this.Filter = filter;
            this.Predicate = predicate;
        }

        public Expression Filter { get; private set; }
        public FilterTerminalType FilterTerminalType { get; private set; }
        public Expression Predicate { get; private set; }

        public override string ToString()
        {
            return OQueryFormatter.Format(this, true);
        }
    }
}
