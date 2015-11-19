using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public class FunctionExpression : DbExpression
    {
        string name;
        ReadOnlyCollection<Expression> arguments;

        public FunctionExpression(Type type, string name, IEnumerable<Expression> arguments)
            : base(DbExpressionType.Function, type)
        {
            this.name = name;
            this.arguments = arguments.ToReadOnly();
        }

        public string Name
        {
            get { return this.name; }
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get { return this.arguments; }
        }
    }
}
