using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Queryable
{
    public class ClientJoinExpression : DbExpression
    {
        ReadOnlyCollection<Expression> outerKey;
        ReadOnlyCollection<Expression> innerKey;
        ProjectionExpression projection;

        public ClientJoinExpression(ProjectionExpression projection, IEnumerable<Expression> outerKey, IEnumerable<Expression> innerKey)
            : base(DbExpressionType.ClientJoin, projection.Type)
        {
            this.outerKey = outerKey.ToReadOnly();
            this.innerKey = innerKey.ToReadOnly();
            this.projection = projection;
        }

        public ReadOnlyCollection<Expression> OuterKey
        {
            get { return this.outerKey; }
        }

        public ReadOnlyCollection<Expression> InnerKey
        {
            get { return this.innerKey; }
        }

        public ProjectionExpression Projection
        {
            get { return this.projection; }
        }
    }
}
