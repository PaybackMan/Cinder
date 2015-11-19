using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public abstract class DbExpression : Expression
    {
        protected DbExpression(DbExpressionType eType, Type type)
            : base((ExpressionType)eType, type)
        {
        }

        public override string ToString()
        {
            return DbExpressionWriter.WriteToString(this);
        }
    }
}
