using SHS.Sage.Common.Linq;
using SHS.Sage.Common.Linq.Expressions;
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
    /// Removes one or more SelectExpression's by rewriting the expression tree to not include them, promoting
    /// their from clause expressions and rewriting any field expressions that may have referenced them to now
    /// reference the underlying data directly.
    /// </summary>
    public class OSubqueryRemover : DbExpressionVisitor
    {
        HashSet<SelectExpression> selectsToRemove;
        Dictionary<IdentifiableAlias, Dictionary<string, Expression>> map;

        private OSubqueryRemover(IEnumerable<SelectExpression> selectsToRemove)
        {
            this.selectsToRemove = new HashSet<SelectExpression>(selectsToRemove);
            this.map = this.selectsToRemove.ToDictionary(d => d.Alias, d => d.Fields.ToDictionary(d2 => d2.Name, d2 => d2.Expression));
        }

        public static SelectExpression Remove(SelectExpression outerSelect, params SelectExpression[] selectsToRemove)
        {
            return Remove(outerSelect, (IEnumerable<SelectExpression>)selectsToRemove);
        }

        public static SelectExpression Remove(SelectExpression outerSelect, IEnumerable<SelectExpression> selectsToRemove)
        {
            return (SelectExpression)new OSubqueryRemover(selectsToRemove).Visit(outerSelect);
        }

        public static ProjectionExpression Remove(ProjectionExpression projection, params SelectExpression[] selectsToRemove)
        {
            return Remove(projection, (IEnumerable<SelectExpression>)selectsToRemove);
        }

        public static ProjectionExpression Remove(ProjectionExpression projection, IEnumerable<SelectExpression> selectsToRemove)
        {
            return (ProjectionExpression)new OSubqueryRemover(selectsToRemove).Visit(projection);
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
            return exp;
        }

        private Expression VisitAssociationFilter(OAssociationFilterExpression exp)
        {
            return exp;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (this.selectsToRemove.Contains(select))
            {
                return this.Visit(select.From);
            }
            else
            {
                return base.VisitSelect(select);
            }
        }

        protected override Expression VisitField(FieldExpression field)
        {
            Dictionary<string, Expression> nameMap;
            if (this.map.TryGetValue(field.Alias, out nameMap))
            {
                Expression expr;
                if (nameMap.TryGetValue(field.Name, out expr))
                {
                    return this.Visit(expr);
                }
                throw new Exception("Reference to undefined field");
            }
            return field;
        }
    }
}
