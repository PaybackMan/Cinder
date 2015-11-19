using SHS.Sage.Linq.Expressions;
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
    public abstract class QueryMapper
    {
        public abstract QueryMapping Mapping { get; }
        public abstract QueryTranslator Translator { get; }

        /// <summary>
        /// Get a query expression that selects all entities from a table
        /// </summary>
        /// <param name="rowType"></param>
        /// <returns></returns>
        public abstract ProjectionExpression GetQueryExpression(IMappedEntity entity);

        /// <summary>
        /// Gets an expression that constructs an entity instance relative to a root.
        /// The root is most often a IdentifiableExpression, but may be any other experssion such as
        /// a ConstantExpression.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract EntityExpression GetEntityExpression(Expression root, IMappedEntity entity);

        /// <summary>
        /// Get an expression for a mapped property relative to a root expression. 
        /// The root is either a IdentifiableExpression or an expression defining an entity instance.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public abstract Expression GetMemberExpression(Expression root, IMappedEntity entity, MemberInfo member);

        /// <summary>
        /// Get an expression that represents the insert operation for the specified instance.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance">The instance to insert.</param>
        /// <param name="selector">A lambda expression that computes a return value from the operation.</param>
        /// <returns></returns>
        public abstract Expression GetInsertExpression(IMappedEntity entity, Expression instance, LambdaExpression selector);

        /// <summary>
        /// Get an expression that represents the update operation for the specified instance.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance"></param>
        /// <param name="updateCheck"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public abstract Expression GetUpdateExpression(IMappedEntity entity, Expression instance, LambdaExpression updateCheck, LambdaExpression selector);

        /// <summary>
        /// Get an expression that represents the delete operation for the specified instance.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance"></param>
        /// <param name="deleteCheck"></param>
        /// <returns></returns>
        public abstract Expression GetDeleteExpression(IMappedEntity entity, Expression instance, LambdaExpression deleteCheck);

        /// <summary>
        /// Recreate the type projection with the additional members included
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fnIsIncluded"></param>
        /// <returns></returns>
        public abstract EntityExpression IncludeMembers(EntityExpression entity, Func<MemberInfo, bool> fnIsIncluded);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool HasIncludedMembers(EntityExpression entity);

        /// <summary>
        /// Apply mapping to a sub query expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression ApplyMapping(Expression expression)
        {
            return QueryBinder.Bind(this, expression);
        }

        /// <summary>
        /// Apply mapping translations to this expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            // convert references to LINQ operators into query specific nodes
            expression = QueryBinder.Bind(this, expression);

            // move aggregate computations so they occur in same select as group-by
            expression = AggregateRewriter.Rewrite(this.Translator.Linguist.Language, expression);

            // do reduction so duplicate association's are likely to be clumped together
            expression = UnusedFieldRemover.Remove(expression);
            expression = RedundantFieldRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);
            expression = RedundantJoinRemover.Remove(expression);

            // convert references to association properties into correlated queries
            var bound = RelationshipBinder.Bind(this, expression);
            if (bound != expression)
            {
                expression = bound;
                // clean up after ourselves! (multiple references to same association property)
                expression = RedundantFieldRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
            }

            // rewrite comparision checks between entities and multi-valued constructs
            expression = ComparisonRewriter.Rewrite(this.Mapping, expression);

            return expression;
        }
    }
}
