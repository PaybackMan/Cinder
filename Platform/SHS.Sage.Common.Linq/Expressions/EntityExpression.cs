using SHS.Sage.Linq.Mapping;
using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    public class EntityExpression : DbExpression
    {
        IMappedEntity entity;
        Expression expression;

        public EntityExpression(IMappedEntity entity, Expression expression)
            : base(DbExpressionType.Entity, expression.Type)
        {
            this.entity = entity;
            this.expression = expression;
        }

        public IMappedEntity Entity
        {
            get { return this.entity; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }
    }
}
