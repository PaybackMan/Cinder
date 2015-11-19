using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Queryable
{
    /// <summary>
    /// A custom expression node used to represent a SQL SELECT expression
    /// </summary>
    public class SelectExpression : AliasedExpression
    {
        ReadOnlyCollection<FieldDeclaration> fields;
        bool isDistinct;
        Expression from;
        Expression where;
        ReadOnlyCollection<OrderExpression> orderBy;
        ReadOnlyCollection<Expression> groupBy;
        Expression take;
        Expression skip;
        bool reverse;

        public SelectExpression(
            IdentifiableAlias alias,
            IEnumerable<FieldDeclaration> fields,
            Expression from,
            Expression where,
            IEnumerable<OrderExpression> orderBy,
            IEnumerable<Expression> groupBy,
            bool isDistinct,
            Expression skip,
            Expression take,
            bool reverse
            )
            : base(DbExpressionType.Select, typeof(void), alias)
        {
            this.fields = fields.ToReadOnly();
            this.isDistinct = isDistinct;
            this.from = from;
            this.where = where;
            this.orderBy = orderBy.ToReadOnly();
            this.groupBy = groupBy.ToReadOnly();
            this.take = take;
            this.skip = skip;
            this.reverse = reverse;
        }
        public SelectExpression(
            IdentifiableAlias alias,
            IEnumerable<FieldDeclaration> fields,
            Expression from,
            Expression where,
            IEnumerable<OrderExpression> orderBy,
            IEnumerable<Expression> groupBy
            )
            : this(alias, fields, from, where, orderBy, groupBy, false, null, null, false)
        {
        }
        public SelectExpression(
            IdentifiableAlias alias, IEnumerable<FieldDeclaration> fields,
            Expression from, Expression where
            )
            : this(alias, fields, from, where, null, null)
        {
        }
        public ReadOnlyCollection<FieldDeclaration> Fields
        {
            get { return this.fields; }
        }
        public Expression From
        {
            get { return this.from; }
        }
        public Expression Where
        {
            get { return this.where; }
        }
        public ReadOnlyCollection<OrderExpression> OrderBy
        {
            get { return this.orderBy; }
        }
        public ReadOnlyCollection<Expression> GroupBy
        {
            get { return this.groupBy; }
        }
        public bool IsDistinct
        {
            get { return this.isDistinct; }
        }
        public Expression Skip
        {
            get { return this.skip; }
        }
        public Expression Take
        {
            get { return this.take; }
        }
        public bool IsReverse
        {
            get { return this.reverse; }
        }
        public string QueryText
        {
            get { return SqlFormatter.Format(this, true); }
        }
    }
}
