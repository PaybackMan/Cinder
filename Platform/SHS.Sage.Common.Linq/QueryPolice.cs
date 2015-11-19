using SHS.Sage.Linq.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class QueryPolice
    {
        QueryPolicy policy;
        QueryTranslator translator;

        public QueryPolice(QueryPolicy policy, QueryTranslator translator)
        {
            this.policy = policy;
            this.translator = translator;
        }

        public QueryPolicy Policy
        {
            get { return this.policy; }
        }

        public QueryTranslator Translator
        {
            get { return this.translator; }
        }

        public virtual Expression ApplyPolicy(Expression expression, MemberInfo member)
        {
            return expression;
        }

        /// <summary>
        /// Provides policy specific query translations.  This is where choices about inclusion of related objects and how
        /// heirarchies are materialized affect the definition of the queries.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            // add included relationships to client projection
            var rewritten = RelationshipIncluder.Include(this.translator.Mapper, expression);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = UnusedFieldRemover.Remove(expression);
                expression = RedundantFieldRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
            }

            // convert any singleton (1:1 or n:1) projections into server-side joins (cardinality is preserved)
            rewritten = SingletonProjectionRewriter.Rewrite(this.translator.Linguist.Language, expression);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = UnusedFieldRemover.Remove(expression);
                expression = RedundantFieldRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
            }

            // convert projections into client-side joins
            rewritten = ClientJoinedProjectionRewriter.Rewrite(this.policy, this.translator.Linguist.Language, expression);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = UnusedFieldRemover.Remove(expression);
                expression = RedundantFieldRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
            }

            return expression;
        }

        /// <summary>
        /// Converts a query into an execution plan.  The plan is an function that executes the query and builds the
        /// resulting objects.
        /// </summary>
        /// <param name="projection"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public virtual Expression BuildExecutionPlan(Expression query, Expression provider)
        {
            return ExecutionBuilder.Build(this.translator.Linguist, this.policy, query, provider);
        }
    }
}
