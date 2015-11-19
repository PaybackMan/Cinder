using SHS.Sage.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SHS.Sage.Linq
{
    public class OFieldFinder : DbExpressionVisitor
    {
        List<FieldExpression> _fields;

        protected override Expression VisitField(FieldExpression field)
        {
            _fields.Add(field);
            return base.VisitField(field);
        }

        public IEnumerable<FieldExpression> Find(Expression expression)
        {
            _fields = new List<FieldExpression>();
            this.Visit(expression);
            return Found;
        }

        public IEnumerable<FieldExpression> Found { get { return _fields.ToReadOnly(); } }
    }
}
