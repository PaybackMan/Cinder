using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Translates accesses to relationship members into projections or joins
    /// </summary>
    public class RelationshipBinder : DbExpressionVisitor
    {
        QueryMapper mapper;
        QueryMapping mapping;
        QueryLanguage language;
        Expression currentFrom;

        protected RelationshipBinder(QueryMapper mapper)
        {
            this.mapper = mapper;
            this.mapping = mapper.Mapping;
            this.language = mapper.Translator.Linguist.Language;
        }

        public static Expression Bind(QueryMapper mapper, Expression expression)
        {
            return new RelationshipBinder(mapper).Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            Expression saveCurrentFrom = this.currentFrom;
            this.currentFrom = this.VisitSource(select.From);
            try
            {
                Expression where = this.Visit(select.Where);
                ReadOnlyCollection<OrderExpression> orderBy = this.VisitOrderBy(select.OrderBy);
                ReadOnlyCollection<Expression> groupBy = this.VisitExpressionList(select.GroupBy);
                Expression skip = this.Visit(select.Skip);
                Expression take = this.Visit(select.Take);
                ReadOnlyCollection<FieldDeclaration> Fields = this.VisitFieldDeclarations(select.Fields);
                if (this.currentFrom != select.From
                    || where != select.Where
                    || orderBy != select.OrderBy
                    || groupBy != select.GroupBy
                    || take != select.Take
                    || skip != select.Skip
                    || Fields != select.Fields
                    )
                {
                    return new SelectExpression(select.Alias, Fields, this.currentFrom, where, orderBy, groupBy, select.IsDistinct, skip, take, select.IsReverse);
                }
                return select;
            }
            finally
            {
                this.currentFrom = saveCurrentFrom;
            }
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            Expression source = this.Visit(m.Expression);
            EntityExpression ex = source as EntityExpression;

            if (ex != null && this.mapping.IsRelationship(ex.Entity, m.Member))
            {
                ProjectionExpression projection = (ProjectionExpression)this.Visit(this.mapper.GetMemberExpression(source, ex.Entity, m.Member));
                if (this.currentFrom != null && this.mapping.IsSingletonRelationship(ex.Entity, m.Member))
                {
                    // convert singleton associations directly to OUTER APPLY
                    projection = this.language.AddOuterJoinTest(projection);
                    Expression newFrom = new JoinExpression(JoinType.OuterApply, this.currentFrom, projection.Select, null);
                    this.currentFrom = newFrom;
                    return projection.Projector;
                }
                return projection;
            }
            else
            {
                Expression result = QueryBinder.BindMember(source, m.Member);
                MemberExpression mex = result as MemberExpression;
                DynamicExpression dex = result as DynamicExpression;

                if ((mex != null && mex.Member == m.Member && mex.Expression == m.Expression)
                    || (dex != null
                    && ((GetMemberBinder)dex.Binder).Name == m.Member.Name)
                    && (dex.Arguments[0] == source || dex.Arguments[0] == ((UnaryExpression)source).Operand))
                {
                    return m;
                }
                return result;
            }
        }
    }
}
