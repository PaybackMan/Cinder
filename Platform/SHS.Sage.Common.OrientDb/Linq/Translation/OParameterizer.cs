using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHS.Sage.Linq.Expressions.Command;
using System.Linq.Expressions;
using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;

namespace SHS.Sage.Linq.Translation
{
    public class OParameterizer : Parameterizer
    {
        protected OParameterizer(OQueryLanguage language) : base(language)
        {
        }

        public new static Expression Parameterize(QueryLanguage language, Expression expression)
        {
            return new OParameterizer((OQueryLanguage)language).Visit(expression);
        }

        protected override Expression VisitInsert(InsertCommandExpression insert)
        {
            var oInsert = insert as OInsertCommandExpression;
            if (oInsert == null) base.VisitInsert(insert);

            var table = (IdentifiableExpression)this.Visit(oInsert.Identifiable);
            var assignments = this.VisitFieldAssignments(oInsert.Assignments);
            var projection = this.VisitProjection(oInsert.Projection);
            return this.UpdateInsert(oInsert, table, assignments, (ProjectionExpression)projection);
        }

        protected virtual InsertCommandExpression UpdateInsert(OInsertCommandExpression insert, IdentifiableExpression table,
            IEnumerable<FieldAssignment> assignments, ProjectionExpression projection)
        {
            if (table != insert.Identifiable || assignments != insert.Assignments || projection.Projector != insert.Projection.Projector)
            {
                return new OInsertCommandExpression(table, assignments, projection.Projector);
            }
            return insert;
        }

        protected override Expression VisitUpdate(UpdateCommandExpression update)
        {
            var oUpdate = update as OUpdateCommandExpression;
            if (oUpdate == null) return base.VisitUpdate(update);

            var table = (IdentifiableExpression)this.Visit(update.Identifiable);
            var where = this.Visit(update.Where);
            var assignments = this.VisitFieldAssignments(update.Assignments);
            var projection = this.VisitProjection(oUpdate.Projection);
            return this.UpdateUpdate(oUpdate, table, where, assignments, (ProjectionExpression)projection);
        }

        protected virtual UpdateCommandExpression UpdateUpdate(OUpdateCommandExpression update, IdentifiableExpression table, Expression where,
            IEnumerable<FieldAssignment> assignments, ProjectionExpression projection)
        {
            if (table != update.Identifiable || where != update.Where || assignments != update.Assignments || projection.Projector != update.Projection.Projector)
            {
                return new OUpdateCommandExpression(table, where, assignments, projection.Projector);
            }
            return update;
        }
    }
}
