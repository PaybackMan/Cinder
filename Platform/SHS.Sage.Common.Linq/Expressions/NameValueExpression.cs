using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public class NamedValueExpression : DbExpression
    {
        string name;
        StorageType queryType;
        Expression value;

        public NamedValueExpression(string name, StorageType queryType, Expression value)
            : base(DbExpressionType.NamedValue, value.Type)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            //if (queryType == null)
            //throw new ArgumentNullException("queryType");
            if (value == null)
                throw new ArgumentNullException("value");
            this.name = name;
            this.queryType = queryType;
            this.value = value;
        }

        public string Name
        {
            get { return this.name; }
        }

        public StorageType QueryType
        {
            get { return this.queryType; }
        }

        public Expression Value
        {
            get { return this.value; }
        }
    }
}
