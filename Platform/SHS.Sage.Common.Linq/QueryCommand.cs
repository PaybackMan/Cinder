using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class QueryCommand
    {
        string commandText;
        ReadOnlyCollection<QueryParameter> parameters;

        public QueryCommand(string commandText, IEnumerable<QueryParameter> parameters)
            : this(commandText, parameters, true)
        { }

        public QueryCommand(string commandText, IEnumerable<QueryParameter> parameters, bool isIdempotent)
        {
            this.commandText = commandText;
            this.parameters = parameters.ToReadOnly();
            this.IsIdempotent = isIdempotent;
        }

        public string CommandText
        {
            get { return this.commandText; }
        }

        public ReadOnlyCollection<QueryParameter> Parameters
        {
            get { return this.parameters; }
        }

        public bool IsIdempotent { get; set; }
    }
}
