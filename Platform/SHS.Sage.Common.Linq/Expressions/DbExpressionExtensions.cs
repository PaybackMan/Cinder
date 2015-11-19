using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public static class DbExpressionExtensions
    {
        public static bool IsDbExpression(ExpressionType nodeType)
        {
            return (int)nodeType >= (int)DbExpressionType.Identifiable;
        }

        public static SelectExpression SetFields(this SelectExpression select, IEnumerable<FieldDeclaration> fields)
        {
            return new SelectExpression(select.Alias, fields.OrderBy(c => c.Name), select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.IsReverse);
        }

        public static SelectExpression AddField(this SelectExpression select, FieldDeclaration field)
        {
            List<FieldDeclaration> fields = new List<FieldDeclaration>(select.Fields);
            fields.Add(field);
            return select.SetFields(fields);
        }

        public static SelectExpression RemoveField(this SelectExpression select, FieldDeclaration field)
        {
            List<FieldDeclaration> fields = new List<FieldDeclaration>(select.Fields);
            fields.Remove(field);
            return select.SetFields(fields);
        }

        public static string GetAvailableFieldName(this IList<FieldDeclaration> fields, string baseName)
        {
            string name = baseName;
            int n = 0;
            while (!IsUniqueName(fields, name))
            {
                name = baseName + (n++);
            }
            return name;
        }

        private static bool IsUniqueName(IList<FieldDeclaration> fields, string name)
        {
            foreach (var col in fields)
            {
                if (col.Name == name)
                {
                    return false;
                }
            }
            return true;
        }

        public static ProjectionExpression AddOuterJoinTest(this ProjectionExpression proj, QueryLanguage language, Expression expression)
        {
            string colName = proj.Select.Fields.GetAvailableFieldName("Test");
            var colType = language.TypeSystem.GetStorageType(expression.Type);
            SelectExpression newSource = proj.Select.AddField(new FieldDeclaration(colName, expression, colType));
            Expression newProjector =
                new OuterJoinedExpression(
                    new FieldExpression(expression.Type, colType, newSource.Alias, colName),
                    proj.Projector
                    );
            return new ProjectionExpression(newSource, newProjector, proj.Aggregator);
        }

        public static SelectExpression SetDistinct(this SelectExpression select, bool isDistinct)
        {
            if (select.IsDistinct != isDistinct)
            {
                return new SelectExpression(select.Alias, select.Fields, select.From, select.Where, select.OrderBy, select.GroupBy, isDistinct, select.Skip, select.Take, select.IsReverse);
            }
            return select;
        }

        public static SelectExpression SetReverse(this SelectExpression select, bool isReverse)
        {
            if (select.IsReverse != isReverse)
            {
                return new SelectExpression(select.Alias, select.Fields, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, isReverse);
            }
            return select;
        }

        public static SelectExpression SetWhere(this SelectExpression select, Expression where)
        {
            if (where != select.Where)
            {
                return new SelectExpression(select.Alias, select.Fields, select.From, where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.IsReverse);
            }
            return select;
        }

        public static SelectExpression SetOrderBy(this SelectExpression select, IEnumerable<OrderExpression> orderBy)
        {
            return new SelectExpression(select.Alias, select.Fields, select.From, select.Where, orderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.IsReverse);
        }

        public static SelectExpression AddOrderExpression(this SelectExpression select, OrderExpression ordering)
        {
            List<OrderExpression> orderby = new List<OrderExpression>();
            if (select.OrderBy != null)
                orderby.AddRange(select.OrderBy);
            orderby.Add(ordering);
            return select.SetOrderBy(orderby);
        }

        public static SelectExpression RemoveOrderExpression(this SelectExpression select, OrderExpression ordering)
        {
            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                List<OrderExpression> orderby = new List<OrderExpression>(select.OrderBy);
                orderby.Remove(ordering);
                return select.SetOrderBy(orderby);
            }
            return select;
        }

        public static SelectExpression SetGroupBy(this SelectExpression select, IEnumerable<Expression> groupBy)
        {
            return new SelectExpression(select.Alias, select.Fields, select.From, select.Where, select.OrderBy, groupBy, select.IsDistinct, select.Skip, select.Take, select.IsReverse);
        }

        public static SelectExpression AddGroupExpression(this SelectExpression select, Expression expression)
        {
            List<Expression> groupby = new List<Expression>();
            if (select.GroupBy != null)
                groupby.AddRange(select.GroupBy);
            groupby.Add(expression);
            return select.SetGroupBy(groupby);
        }

        public static SelectExpression RemoveGroupExpression(this SelectExpression select, Expression expression)
        {
            if (select.GroupBy != null && select.GroupBy.Count > 0)
            {
                List<Expression> groupby = new List<Expression>(select.GroupBy);
                groupby.Remove(expression);
                return select.SetGroupBy(groupby);
            }
            return select;
        }

        public static SelectExpression SetSkip(this SelectExpression select, Expression skip)
        {
            if (skip != select.Skip)
            {
                return new SelectExpression(select.Alias, select.Fields, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, skip, select.Take, select.IsReverse);
            }
            return select;
        }

        public static SelectExpression SetTake(this SelectExpression select, Expression take)
        {
            if (take != select.Take)
            {
                return new SelectExpression(select.Alias, select.Fields, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, take, select.IsReverse);
            }
            return select;
        }

        public static SelectExpression AddRedundantSelect(this SelectExpression sel, QueryLanguage language, IdentifiableAlias newAlias)
        {
            var newFields =
                from d in sel.Fields
                let qt = (d.Expression is FieldExpression) ? ((FieldExpression)d.Expression).QueryType : language.TypeSystem.GetStorageType(d.Expression.Type)
                select new FieldDeclaration(d.Name, new FieldExpression(d.Expression.Type, qt, newAlias, d.Name), qt);

            var newFrom = new SelectExpression(newAlias, sel.Fields, sel.From, sel.Where, sel.OrderBy, sel.GroupBy, sel.IsDistinct, sel.Skip, sel.Take, sel.IsReverse);
            return new SelectExpression(sel.Alias, newFields, newFrom, null, null, null, false, null, null, false);
        }

        public static SelectExpression RemoveRedundantFrom(this SelectExpression select)
        {
            SelectExpression fromSelect = select.From as SelectExpression;
            if (fromSelect != null)
            {
                return SubqueryRemover.Remove(select, fromSelect);
            }
            return select;
        }

        public static SelectExpression SetFrom(this SelectExpression select, Expression from)
        {
            if (select.From != from)
            {
                return new SelectExpression(select.Alias, select.Fields, from, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.IsReverse);
            }
            return select;
        }
    }
}
