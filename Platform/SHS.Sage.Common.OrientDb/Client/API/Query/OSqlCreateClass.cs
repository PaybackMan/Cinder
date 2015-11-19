﻿using System.Collections.Generic;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;

// syntax: 
// CREATE CLASS <class> 
// [EXTENDS <super-class>] 
// [CLUSTER <clusterId>*]

namespace Orient.Client
{
    public class OSqlCreateClass
    {
        private SqlQuery _sqlQuery = new SqlQuery();
        private Connection _connection;
        private string _className;

        public OSqlCreateClass()
        {
        }

        internal OSqlCreateClass(Connection connection)
        {
            _connection = connection;
        }

        #region Class

        public OSqlCreateClass Class(string className)
        {
            _className = className;
            _sqlQuery.Class(_className);

            return this;
        }

        public OSqlCreateClass Class<T>()
        {
            _className = typeof (T).Name;
            return Class(_className);
        }

        #endregion

        #region Extends

        public OSqlCreateClass Extends(string superClass)
        {
            _sqlQuery.Extends(superClass);

            return this;
        }

        public OSqlCreateClass Extends<T>()
        {
            return Extends(typeof(T).Name);
        }

        #endregion

        public OSqlCreateClass Cluster(short clusterId)
        {
            _sqlQuery.Cluster(clusterId.ToString());

            return this;
        }

        public short Run()
        {
            CommandPayload payload = new CommandPayload();
            payload.Type = CommandPayloadType.Sql;
            payload.Text = ToString();
            payload.NonTextLimit = -1;
            payload.FetchPlan = "";
            payload.SerializedParams = new byte[] { 0 };

            Command operation = new Command();
            operation.OperationMode = OperationMode.Synchronous;
            operation.ClassType = CommandClassType.NonIdempotent;
            operation.CommandPayload = payload;

            OCommandResult result = new OCommandResult(_connection.ExecuteOperation(operation));

            var clusterId = short.Parse(result.ToDocument().GetField<string>("Content"));

            _connection.Database.AddCluster(_className, clusterId);

            return clusterId;
        }

        public override string ToString()
        {
            return _sqlQuery.ToString(QueryType.CreateClass);
        }
    }
}
