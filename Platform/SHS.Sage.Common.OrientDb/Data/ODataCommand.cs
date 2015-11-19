using Orient.Client;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHS.Sage.Linq.Language;
using System.Diagnostics;

namespace SHS.Sage.Data
{
    internal class ODataCommand : DbCommand
    {
        ODatabase _database;
        ODataParameterCollection _params;

        public ODataCommand(ODatabase database)
        {
            _database = database;
            _params = new ODataParameterCollection();
            IsIdempotent = true;
        }

        public ODataCommand(ODatabase database, string commandText, bool isIdempotent)
        {
            _database = database;
            _params = new ODataParameterCollection();
            IsIdempotent = isIdempotent;
            CommandText = commandText;
        }

        internal IOperation CreateOperation()
        {
            CommandPayload payload = new CommandPayload();
            payload.Type = CommandPayloadType.Sql;
            payload.Text = AssignParameters(CommandText);
            payload.NonTextLimit = -1;
            payload.FetchPlan = this.IsIdempotent ? "*:0" : "";
            payload.SerializedParams = new byte[] { 0 };

            var command = new Command();
            command.OperationMode = this.IsIdempotent ? OperationMode.Asynchronous : OperationMode.Synchronous;
            command.ClassType = this.IsIdempotent ? CommandClassType.Idempotent : CommandClassType.NonIdempotent;
            command.CommandPayload = payload;
            Trace.WriteLine(payload.Text);
            return command;
        }

        private string AssignParameters(string commandText)
        {
            foreach(var parm in this.Parameters.OfType<ODataParameter>()) // need to fix string replacement here
            {
                var value = parm.Value.ToOrientValueString(parm.DestinationType);
                commandText = commandText.Replace("{@" + parm.ParameterName + "}", value);
            }
            return commandText;
        }

        public bool IsIdempotent { get; set; }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override string CommandText
        {
            get;
            set;
        }

        public override int CommandTimeout
        {
            get;
            set;
        }

        public override System.Data.CommandType CommandType
        {
            get;
            set;
        }

        protected override DbParameter CreateDbParameter()
        {
            return new ODataParameter();
        }

        protected override DbConnection DbConnection
        {
            get;
            set;
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _params; }
        }

        protected override DbTransaction DbTransaction
        {
            get;
            set;
        }

        public override bool DesignTimeVisible
        {
            get;
            set;
        }

        protected override DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
        {
            return new ODataReader(this);
        }

        public override int ExecuteNonQuery()
        {
            using (var reader = ExecuteDbDataReader(System.Data.CommandBehavior.Default))
            {
                reader.Read();
                return reader.RecordsAffected;
            }
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override System.Data.UpdateRowSource UpdatedRowSource
        {
            get;
            set;
        }

        internal ODatabase Database { get { return _database; } }
        
    }
}
