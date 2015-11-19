using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    /// <summary>
    /// A custom expression node that represents a reference to a field in a SQL query
    /// </summary>
    public class FieldExpression : DbExpression, IEquatable<FieldExpression>
    {
        IdentifiableAlias alias;
        string name;
        StorageType queryType;

        public FieldExpression(Type type, StorageType queryType, IdentifiableAlias alias, string name)
            : base(DbExpressionType.Field, type)
        {
            if (queryType == null)
                throw new ArgumentNullException("queryType");
            if (name == null)
                throw new ArgumentNullException("name");
            this.alias = alias;
            this.name = name;
            this.queryType = queryType;
        }

        public IdentifiableAlias Alias
        {
            get { return this.alias; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public StorageType QueryType
        {
            get { return this.queryType; }
        }

        public override string ToString()
        {
            return this.Alias.ToString() + ".C(" + this.name + ")";
        }

        public override int GetHashCode()
        {
            return alias.GetHashCode() + name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FieldExpression);
        }

        public bool Equals(FieldExpression other)
        {
            return other != null
                && ((object)this) == (object)other
                 || (alias == other.alias && name == other.Name);
        }
    }
}
