using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class VariableDeclaration
    {
        string name;
        StorageType type;
        Expression expression;

        public VariableDeclaration(string name, StorageType type, Expression expression)
        {
            this.name = name;
            this.type = type;
            this.expression = expression;
        }

        public string Name
        {
            get { return this.name; }
        }

        public StorageType QueryType
        {
            get { return this.type; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }
    }
}
