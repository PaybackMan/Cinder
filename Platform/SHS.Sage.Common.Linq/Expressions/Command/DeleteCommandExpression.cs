using SHS.Sage.Linq.Expressions.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class DeleteCommandExpression : CommandExpression
    {
        IdentifiableExpression identifiable;
        Expression where;

        public DeleteCommandExpression(IdentifiableExpression identifiable, Expression where)
            : base(DbExpressionType.Delete, typeof(int))
        {
            this.identifiable = identifiable;
            this.where = where;
        }

        public IdentifiableExpression Identifiable
        {
            get { return this.identifiable; }
        }

        public Expression Where
        {
            get { return this.where; }
        }
    }
}
