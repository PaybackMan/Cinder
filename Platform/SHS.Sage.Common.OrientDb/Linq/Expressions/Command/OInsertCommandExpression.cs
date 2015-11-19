using SHS.Sage.Linq.Expressions.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class OInsertCommandExpression : InsertCommandExpression
    {
        public OInsertCommandExpression(IdentifiableExpression identifiable, IEnumerable<FieldAssignment> assignments, Expression projector)
            : base (identifiable, assignments)
        {
            Projection = new ProjectionExpression(null,
                projector,
                Aggregator.GetAggregator(projector.Type, typeof(IEnumerable<>).MakeGenericType(projector.Type)));
        }

        public ProjectionExpression Projection { get; private set; }
    }
}
