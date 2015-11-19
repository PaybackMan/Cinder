using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Common.Linq.Expressions;

namespace SHS.Sage.Common.Linq
{
    public class ODbExpressionWriter : DbExpressionWriter
    {
        protected ODbExpressionWriter(TextWriter writer, QueryLanguage language) : base(writer, language)
        {
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
                return null;

            switch ((ODbExpressionType)exp.NodeType)
            {
                case ODbExpressionType.AssociationFilter:
                    return this.VisitAssociationFilter(exp);
                default:
                    return base.Visit(exp);
            }
            
        }

        protected virtual Expression VisitAssociationFilter(Expression exp)
        {
            var filter = exp as OAssociationFilterExpression;
            this.Write("Filter Expression TBD");
            return exp;
        }
    }
}
