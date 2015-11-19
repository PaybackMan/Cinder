using SHS.Sage.Linq.Expressions.Queryable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions.Command
{
    public class DeclarationCommand : CommandExpression
    {
        ReadOnlyCollection<VariableDeclaration> variables;
        SelectExpression source;

        public DeclarationCommand(IEnumerable<VariableDeclaration> variables, SelectExpression source)
            : base(DbExpressionType.Declaration, typeof(void))
        {
            this.variables = variables.ToReadOnly();
            this.source = source;
        }

        public ReadOnlyCollection<VariableDeclaration> Variables
        {
            get { return this.variables; }
        }

        public SelectExpression Source
        {
            get { return this.source; }
        }
    }
}
