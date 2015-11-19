using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Queryable
{
    /// <summary>
    /// A custom expression representing the construction of one or more result objects from a 
    /// SQL select expression
    /// </summary>
    public class ProjectionExpression : DbExpression
    {
        SelectExpression select;
        Expression projector;
        LambdaExpression aggregator;
        public ProjectionExpression(SelectExpression source, Expression projector)
            : this(source, projector, null)
        {
        }
        public ProjectionExpression(SelectExpression source, Expression projector, LambdaExpression aggregator)
            : base(DbExpressionType.Projection, aggregator != null ? aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projector.Type))
        {
            this.select = source;
            this.projector = projector;
            this.aggregator = aggregator;
        }
        public SelectExpression Select
        {
            get { return this.select; }
        }
        public Expression Projector
        {
            get { return this.projector; }
        }
        public LambdaExpression Aggregator
        {
            get { return this.aggregator; }
        }
        public bool IsSingleton
        {
            get { return this.aggregator != null && this.aggregator.Body.Type == projector.Type; }
        }
        public override string ToString()
        {
            return DbExpressionWriter.WriteToString(this);
        }
        public string QueryText
        {
            get { return SqlFormatter.Format(select, true); }
        }
    }
}
