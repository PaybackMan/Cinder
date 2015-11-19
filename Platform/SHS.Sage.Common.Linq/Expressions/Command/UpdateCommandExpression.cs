using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class UpdateCommandExpression : CommandExpression
    {
        IdentifiableExpression identifiable;
        Expression where;
        ReadOnlyCollection<FieldAssignment> assignments;

        public UpdateCommandExpression(IdentifiableExpression identifiable, Expression where, IEnumerable<FieldAssignment> assignments)
            : base(DbExpressionType.Update, typeof(int))
        {
            this.identifiable = identifiable;
            this.where = where;
            this.assignments = assignments.ToReadOnly();
        }

        public IdentifiableExpression Identifiable
        {
            get { return this.identifiable; }
        }

        public Expression Where
        {
            get { return this.where; }
        }

        public ReadOnlyCollection<FieldAssignment> Assignments
        {
            get { return this.assignments; }
        }
    }
}
