using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class BlockCommandExpression : CommandExpression
    {
        ReadOnlyCollection<Expression> commands;

        public BlockCommandExpression(IList<Expression> commands)
            : base(DbExpressionType.Block, commands[commands.Count - 1].Type)
        {
            this.commands = commands.ToReadOnly();
        }

        public BlockCommandExpression(params Expression[] commands)
            : this((IList<Expression>)commands)
        {
        }

        public ReadOnlyCollection<Expression> Commands
        {
            get { return this.commands; }
        }
    }
}
