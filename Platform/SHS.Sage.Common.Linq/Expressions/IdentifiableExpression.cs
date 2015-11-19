using SHS.Sage.Linq.Mapping;
using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{

    /// <summary>
    /// A custom expression node that represents a table reference in a SQL query
    /// </summary>
    public class IdentifiableExpression : AliasedExpression
    {
        IMappedEntity entity;
        string name;

        public IdentifiableExpression(IdentifiableAlias alias, IMappedEntity entity, string name)
            : base(DbExpressionType.Identifiable, typeof(void), alias)
        {
            this.entity = entity;
            this.name = name;
        }

        public IMappedEntity Entity
        {
            get { return this.entity; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public override string ToString()
        {
            return "T(" + this.Name + ")";
        }
    }
}
