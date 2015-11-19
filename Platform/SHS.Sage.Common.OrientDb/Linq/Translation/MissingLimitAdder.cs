using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Adds a LIMIT -1 Expression to the root Select statement, if one does not alread exist
    /// </summary>
    public class MissingLimitAdder : DbExpressionVisitor
    {
        private MissingLimitAdder(OQueryPolicy policy)
        {
            this.Policy = policy;
        }

        public static Expression Evaluate(Expression expression, OQueryPolicy policy)
        {
            return new MissingLimitAdder(policy).Visit(expression);
        }

        bool _visited = false;

        public OQueryPolicy Policy { get; private set; }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (!_visited)
            {
                _visited = true;
                if (select.Take == null)
                {
                    Expression take = Visit(Expression.Constant(Policy.Limit));
                    select = new SelectExpression(
                        select.Alias, 
                        select.Fields, 
                        select.From, 
                        select.Where, 
                        select.OrderBy, 
                        select.GroupBy, 
                        select.IsDistinct, 
                        select.Skip, 
                        take, 
                        select.IsReverse);
                }
            }

            return select;
        }
    }
}
