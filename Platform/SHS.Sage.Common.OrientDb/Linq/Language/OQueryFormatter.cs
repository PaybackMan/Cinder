using SHS.Sage.Common.Linq.Expressions;
using SHS.Sage.Linq;
using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Common.Linq.Language
{
    public class OQueryFormatter : SqlFormatter
    {

        protected OQueryFormatter(QueryLanguage language, bool forDebug)
            :base(language, forDebug)
        {

        }

        protected OQueryFormatter(QueryLanguage language)
            : this(language, false)
        {
        }

        public new static string Format(Expression expression, bool forDebug)
        {
            var formatter = new OQueryFormatter(null, forDebug);
            formatter.Visit(expression);
            return formatter.ToString();
        }

        public new static string Format(Expression expression)
        {
            var formatter = new OQueryFormatter(null, false);
            formatter.Visit(expression);
            return formatter.ToString();
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null) return null;

            switch ((ODbExpressionType)exp.NodeType)
            {
                case ODbExpressionType.AssociationFilter:
                    return this.VisitAssociationFilter((OAssociationFilterExpression)exp);
                case ODbExpressionType.AssociationFilterTerminal:
                    return this.VisitAssociationFilterTerminal((OAssociationFilterTerminalExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }

        private Expression VisitAssociationFilterTerminal(OAssociationFilterTerminalExpression exp)
        {
            this.Visit(exp.Filter);

            switch(exp.FilterTerminalType)
            {
                case FilterTerminalType.Count:
                    this.Write(".size()");
                    break;
            }
            
            return exp;
        }

        private Expression VisitAssociationFilter(OAssociationFilterExpression exp)
        {
            var source = exp.Source as FieldExpression;

            this.Write(source.Name); // this probably has very limited support

            if (exp.Origin == AssociationOrigin.Source)
            {
                this.Write(".outE()");
            }
            else
            {
                this.Write(".inE()");
            }

            var edgeType = exp.Type.GetGenericArguments()[0];
            if (edgeType.Implements<IProxyIdentifiable>())
            {
                edgeType = edgeType.BaseType;
            }
            if (!edgeType.Equals(typeof(IAssociation)))
            {
                this.Write(string.Format("[@class='{0}']", edgeType.Name));
            }

            this.Write("[");
            this.Visit(exp.Predicate); // this needs to be changed
            this.Write("]");
            return exp;
        }
    }
}
