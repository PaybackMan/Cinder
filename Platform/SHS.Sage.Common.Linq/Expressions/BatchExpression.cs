using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public class BatchExpression : Expression
    {
        Expression input;
        LambdaExpression operation;
        Expression batchSize;
        Expression stream;

        public BatchExpression(Expression input, LambdaExpression operation, Expression batchSize, Expression stream)
            : base((ExpressionType)DbExpressionType.Batch, typeof(IEnumerable<>).MakeGenericType(operation.Body.Type))
        {
            this.input = input;
            this.operation = operation;
            this.batchSize = batchSize;
            this.stream = stream;
        }

        public Expression Input
        {
            get { return this.input; }
        }

        public LambdaExpression Operation
        {
            get { return this.operation; }
        }

        public Expression BatchSize
        {
            get { return this.batchSize; }
        }

        public Expression Stream
        {
            get { return this.stream; }
        }
    }
}
