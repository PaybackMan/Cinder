using SHS.Sage.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Command;
using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Common.Linq.Expressions;


namespace SHS.Sage.Linq.Translation
{
    public class OQueryBinder : QueryBinder
    {
        protected OQueryBinder(QueryMapper mapper, Expression root)
            : base(mapper, root){ }

        public new static Expression Bind(QueryMapper mapper, Expression expression)
        {
            return new OQueryBinder(mapper, expression).Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType.Equals(typeof(OQueryable)))
            {
                switch (m.Method.Name)
                {
                    case "TargetOf":
                    case "SourceOf":
                        {
                            var origin = m.Method.Name.Equals("SourceOf") ? AssociationOrigin.Source : AssociationOrigin.Target;
                            return this.BindAssociationFilter(origin, m.Type, m.Arguments[0], GetLambda(m.Arguments[1]));
                        }
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                return base.VisitMethodCall(m);
            }
        }

        private Expression BindAssociationFilter(AssociationOrigin origin, Type type, Expression source, LambdaExpression predicate)
        {
            var TAssociation = predicate.Parameters[0].Type;
            var sourceMember = Expression.MakeMemberAccess(
                predicate.Parameters[0],
                TAssociation.GetPublicProperty(origin.ToString()));
            var sourceEquals = Expression.Equal(Expression.Constant(source), sourceMember);
            var lambdaType = predicate.Type;
            var sourcePredicate = Expression.Lambda(lambdaType, sourceEquals, predicate.Parameters);

            var completePredicate = Expression.Lambda(
                lambdaType,
                Expression.And(sourcePredicate.Body, predicate.Body),
                predicate.Parameters);

            ProjectionExpression projection = this.VisitSequence(
                this.Mapper.GetQueryExpression(
                    this.Mapper.Mapping.GetEntity(type.GetGenericArguments()[0], this.RepositoryType)));
            this.Map[predicate.Parameters[0]] = projection.Projector;
            Expression where = this.Visit(predicate.Body);
            var alias = this.GetNextAlias();
            ProjectedFields pc = this.ProjectFields(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Fields, projection.Select, where),
                pc.Projector
                );
        }

        protected override Expression VisitInsert(InsertCommandExpression insert)
        {
            var oInsert = insert as OInsertCommandExpression;
            if (oInsert == null) return base.VisitInsert(insert);

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
