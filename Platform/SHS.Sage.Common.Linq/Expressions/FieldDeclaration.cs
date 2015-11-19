using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    /// <summary>
    /// A declaration of a field in a SQL SELECT expression
    /// </summary>
    public class FieldDeclaration
    {
        string name;
        Expression expression;
        StorageType queryType;

        public FieldDeclaration(string name, Expression expression, StorageType queryType)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (queryType == null)
                throw new ArgumentNullException("queryType");
            this.name = name;
            this.expression = expression;
            this.queryType = queryType;
        }

        public string Name
        {
            get { return this.name; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }

        public StorageType QueryType
        {
            get { return this.queryType; }
        }
    }
}
