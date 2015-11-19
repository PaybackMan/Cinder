using SHS.Sage.Common.Linq.Translation;
using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Command;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Translation;
using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Mapping
{
    public class OTypeMapper : BasicMapper
    {
        Type _repoType;
        public OTypeMapper(BasicMapping mapping, QueryTranslator translator) : base(mapping, translator)
        {
            _repoType = ((OTypeMapping)mapping).RepositoryType;
        }

        public override Expression Translate(Expression expression)
        {
            // convert references to LINQ operators into query specific nodes
            expression = OQueryBinder.Bind(this, expression);

            // move aggregate computations so they occur in same select as group-by
            expression = OAggregateRewriter.Rewrite(this.Translator.Linguist.Language, expression);

            // do reduction so duplicate association's are likely to be clumped together
            expression = OUnusedFieldRemover.Remove(expression);
            expression = ORedundantFieldRemover.Remove(expression);
            expression = ORedundantSubqueryRemover.Remove(expression);
            expression = ORedundantJoinRemover.Remove(expression);

            // convert references to association properties into correlated queries
            var bound = ORelationshipBinder.Bind(this, expression);
            if (bound != expression)
            {
                expression = bound;
                // clean up after ourselves! (multiple references to same association property)
                expression = ORedundantFieldRemover.Remove(expression);
                expression = ORedundantJoinRemover.Remove(expression);
            }

            // rewrite comparision checks between entities and multi-valued constructs
            expression = OComparisonRewriter.Rewrite(this.Mapping, expression);

            return expression;
        }

        protected override Expression BuildEntityExpression(IMappedEntity entity, IList<EntityAssignment> assignments)
        {
            if (entity.EntityType.Equals(typeof(IIdentifiable)) 
                || entity.EntityType.Equals(typeof(IThing)))
            {
                return base.BuildEntityExpression(new OMappingEntity(typeof(_Thing), _repoType), assignments); // switch it to a constructable type
            }
            else if (entity.EntityType.Equals(typeof(IAssociation)))
            {
                return base.BuildEntityExpression(new OMappingEntity(typeof(_Association), _repoType), assignments); // switch it to a constructable type
            }
            else return base.BuildEntityExpression(entity, assignments);
        }

        public override Expression GetInsertExpression(IMappedEntity entity, Expression instance, LambdaExpression selector)
        {
            var IdentifiableAlias = new IdentifiableAlias();
            var table = new IdentifiableExpression(IdentifiableAlias, entity, ((OTypeMapping)this.Mapping).GetTableName(entity));
            var assignments = this.GetFieldAssignments(table, instance, entity, (e, m) => !((OTypeMapping)this.Mapping).IsGenerated(e, m));

            if (selector != null)
            {
                return CreateInsertCommand(entity, instance, selector, assignments);
            }

            return new InsertCommandExpression(table, assignments);
        }

        protected virtual OInsertCommandExpression CreateInsertCommand(IMappedEntity entity, Expression instance, LambdaExpression selector, IEnumerable<FieldAssignment> assigments)
        {
            var identifiableAlias = new IdentifiableAlias();
            var tex = new IdentifiableExpression(identifiableAlias, entity, ((OTypeMapping)this.Mapping).GetTableName(entity));
            Expression typeProjector = this.GetEntityExpression(tex, entity);
            Expression selection = DbExpressionReplacer.Replace(selector.Body, selector.Parameters[0], typeProjector);
            IdentifiableAlias newAlias = new IdentifiableAlias();
            var pc = FieldProjector.ProjectFields(this.Translator.Linguist.Language, selection, null, newAlias, identifiableAlias);

            return new OInsertCommandExpression(tex, assigments, pc.Projector);
        }

        public override Expression GetUpdateExpression(IMappedEntity entity, Expression instance, LambdaExpression updateCheck, LambdaExpression selector)
        {
            var IdentifiableAlias = new IdentifiableAlias();
            var table = new IdentifiableExpression(IdentifiableAlias, entity, ((OTypeMapping)this.Mapping).GetTableName(entity));

            var where = this.GetIdentityCheck(table, entity, instance);
            if (updateCheck != null)
            {
                Expression typeProjector = this.GetEntityExpression(table, entity);
                Expression pred = DbExpressionReplacer.Replace(updateCheck.Body, updateCheck.Parameters[0], typeProjector);
                where = where.And(pred);
            }

            var assignments = this.GetFieldAssignments(table, instance, entity, (e, m) => ((OTypeMapping)this.Mapping).IsUpdatable(e, m));

            Expression update = new UpdateCommandExpression(table, where, assignments);

            if (selector != null)
            {
                return CreateUpdateCommand(entity, instance, selector, where, assignments);
            }
            else
            {
                return update;
            }
        }

        private OUpdateCommandExpression CreateUpdateCommand(IMappedEntity entity, 
            Expression instance, LambdaExpression selector, Expression where, IEnumerable<FieldAssignment> assignments)
        {
            var identifiableAlias = new IdentifiableAlias();
            var tex = new IdentifiableExpression(identifiableAlias, entity, ((OTypeMapping)this.Mapping).GetTableName(entity));
            Expression typeProjector = this.GetEntityExpression(tex, entity);
            Expression selection = DbExpressionReplacer.Replace(selector.Body, selector.Parameters[0], typeProjector);
            IdentifiableAlias newAlias = new IdentifiableAlias();
            var pc = FieldProjector.ProjectFields(this.Translator.Linguist.Language, selection, null, newAlias, identifiableAlias);

            return new OUpdateCommandExpression(tex, where, assignments, pc.Projector);
        }
    }
}
