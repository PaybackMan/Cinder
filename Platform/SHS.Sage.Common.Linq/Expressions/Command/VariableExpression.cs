using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class VariableExpression : Expression
    {
        string name;
        StorageType queryType;

        public VariableExpression(string name, Type type, StorageType queryType)
            : base((ExpressionType)DbExpressionType.Variable, type)
        {
            this.name = name;
            this.queryType = queryType;
        }

        public string Name
        {
            get { return this.name; }
        }

        public StorageType QueryType
        {
            get { return this.queryType; }
        }
    }
}
