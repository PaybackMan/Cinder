using SHS.Sage.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Common.Linq.Expressions
{
    public class OAssociationFilterBinaryExpression : DbExpression
    {
        public OAssociationFilterBinaryExpression(ExpressionType filterType, Expression left, Expression right, MethodInfo method, Type type)
            : base((DbExpressionType)filterType, type)
        {
            if (filterType != ExpressionType.Equal)
                throw new NotSupportedException("Association filters only support equality comparisons.");
            this.Left = left;
            this.Right = right;
            this.Method = method;
        }

        public Expression Left { get; private set; }
        public MethodInfo Method { get; private set; }
        public Expression Right { get; private set; }
    }
}
