using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class InsertCommandExpression : CommandExpression
    {
        IdentifiableExpression identifiable;
        ReadOnlyCollection<FieldAssignment> assignments;

        public InsertCommandExpression(IdentifiableExpression identifiable, IEnumerable<FieldAssignment> assignments)
            : base(DbExpressionType.Insert, typeof(int))
        {
            this.identifiable = identifiable;
            this.assignments = assignments.ToReadOnly();
        }

        public IdentifiableExpression Identifiable
        {
            get { return this.identifiable; }
        }

        public ReadOnlyCollection<FieldAssignment> Assignments
        {
            get { return this.assignments; }
        }
    }
}
