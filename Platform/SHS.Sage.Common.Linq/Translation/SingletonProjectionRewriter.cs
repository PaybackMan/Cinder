﻿using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Command;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Rewrites nested singleton projection into server-side joins
    /// </summary>
    public class SingletonProjectionRewriter : DbExpressionVisitor
    {
        QueryLanguage language;
        bool isTopLevel = true;
        SelectExpression currentSelect;

        private SingletonProjectionRewriter(QueryLanguage language)
        {
            this.language = language;
        }

        public static Expression Rewrite(QueryLanguage language, Expression expression)
        {
            return new SingletonProjectionRewriter(language).Visit(expression);
        }

        protected override Expression VisitClientJoin(ClientJoinExpression join)
        {
            // treat client joins as new top level
            var saveTop = this.isTopLevel;
            var saveSelect = this.currentSelect;
            this.isTopLevel = true;
            this.currentSelect = null;
            Expression result = base.VisitClientJoin(join);
            this.isTopLevel = saveTop;
            this.currentSelect = saveSelect;
            return result;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            if (isTopLevel)
            {
                isTopLevel = false;
                this.currentSelect = proj.Select;
                Expression projector = this.Visit(proj.Projector);
                if (projector != proj.Projector || this.currentSelect != proj.Select)
                {
                    return new ProjectionExpression(this.currentSelect, projector, proj.Aggregator);
                }
                return proj;
            }

            if (proj.IsSingleton && this.CanJoinOnServer(this.currentSelect))
            {
                IdentifiableAlias newAlias = new IdentifiableAlias();
                this.currentSelect = this.currentSelect.AddRedundantSelect(this.language, newAlias);

                // remap any references to the outer select to the new alias;
                SelectExpression source = (SelectExpression)FieldMapper.Map(proj.Select, newAlias, this.currentSelect.Alias);

                // add outer-join test
                ProjectionExpression pex = this.language.AddOuterJoinTest(new ProjectionExpression(source, proj.Projector));

                var pc = FieldProjector.ProjectFields(this.language, pex.Projector, this.currentSelect.Fields, this.currentSelect.Alias, newAlias, proj.Select.Alias);

                JoinExpression join = new JoinExpression(JoinType.OuterApply, this.currentSelect.From, pex.Select, null);

                this.currentSelect = new SelectExpression(this.currentSelect.Alias, pc.Fields, join, null);
                return this.Visit(pc.Projector);
            }

            var saveTop = this.isTopLevel;
            var saveSelect = this.currentSelect;
            this.isTopLevel = true;
            this.currentSelect = null;
            Expression result = base.VisitProjection(proj);
            this.isTopLevel = saveTop;
            this.currentSelect = saveSelect;
            return result;
        }

        private bool CanJoinOnServer(SelectExpression select)
        {
            // can add singleton (1:0,1) join if no grouping/aggregates or distinct
            return !select.IsDistinct
                && (select.GroupBy == null || select.GroupBy.Count == 0)
                && !AggregateChecker.HasAggregates(select);
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery)
        {
            return subquery;
        }

        protected override Expression VisitCommand(CommandExpression command)
        {
            this.isTopLevel = true;
            return base.VisitCommand(command);
        }
    }
}