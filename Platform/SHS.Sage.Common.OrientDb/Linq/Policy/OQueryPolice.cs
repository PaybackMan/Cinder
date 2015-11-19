using SHS.Sage.Common.Linq.Translation;
using SHS.Sage.Linq.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Policy
{
    public class OQueryPolice : QueryPolice
    {
        public OQueryPolice(QueryPolicy policy, QueryTranslator translator) : base(policy, translator) { }

        public override Expression Translate(Expression expression)
        {
            var rewritten = ORelationshipIncluder.Include(this.Translator.Mapper, expression);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = OUnusedFieldRemover.Remove(expression);
                expression = ORedundantFieldRemover.Remove(expression);
                expression = ORedundantSubqueryRemover.Remove(expression);
                expression = ORedundantJoinRemover.Remove(expression);
            }

            // convert projections into client-side joins
            rewritten = ClientJoinedProjectionRewriter.Rewrite(this.Policy, this.Translator.Linguist.Language, expression);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = OUnusedFieldRemover.Remove(expression);
                expression = ORedundantFieldRemover.Remove(expression);
                expression = ORedundantSubqueryRemover.Remove(expression);
                expression = ORedundantJoinRemover.Remove(expression);
            }

            return expression;
        }

        public override Expression BuildExecutionPlan(Expression query, Expression provider)
        {
            return OExecutionBuilder.Build(this.Translator.Linguist, this.Policy, query, provider);
        }
    }
}
