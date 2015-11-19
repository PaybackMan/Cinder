using System.Collections.Generic;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;

// syntax:
// TRAVERSE <[class.]field>|*|any()|all()
// [FROM <target>]
// [LET <Assignment>*]
// WHILE <condition>
// [LIMIT <max-records>]
// [STRATEGY <strategy>]

namespace Orient.Client
{
    public class OSqlTraverse
    {
        private SqlQuery _sqlQuery = new SqlQuery();
        private Connection _connection;

        public OSqlTraverse()
        {
        }

        internal OSqlTraverse(Connection connection)
        {
            _connection = connection;
        }

        #region Select

        public enum FieldSelection
        {
            Any,
            All,
            Wildcard
        }

        public OSqlTraverse Traverse(FieldSelection selection)
        {
            switch(selection)
            {
                default:
                    {
                        _sqlQuery.Traverse(selection.ToString().ToLower() + "()");
                        break;
                    }
                case FieldSelection.Wildcard:
                    {
                        _sqlQuery.Traverse("*");
                        break;
                    }
            }

            return this;
        }

        public OSqlTraverse Traverse(params string[] projections)
        {
            _sqlQuery.Traverse(projections);

            return this;
        }

        public OSqlTraverse Also(string projection)
        {
            _sqlQuery.Also(projection);

            return this;
        }

        /*public OSqlSelect First()
        {
            _sqlQuery.Surround("first");

            return this;
        }*/

        public OSqlTraverse Nth(int index)
        {
            _sqlQuery.Nth(index);

            return this;
        }

        public OSqlTraverse As(string alias)
        {
            _sqlQuery.As(alias);

            return this;
        }

        #endregion

        #region From

        public OSqlTraverse From(string target)
        {
            _sqlQuery.From(target);

            return this;
        }

        public OSqlTraverse From(ORID orid)
        {
            _sqlQuery.From(orid);

            return this;
        }

        public OSqlTraverse From(ODocument document)
        {
            if ((document.ORID == null) && string.IsNullOrEmpty(document.OClassName))
            {
                throw new OException(OExceptionType.Query, "Document doesn't contain ORID or OClassName value.");
            }

            _sqlQuery.From(document);

            return this;
        }

        public OSqlTraverse From<T>()
        {
            return From(typeof(T).Name);
        }

        #endregion

        #region Where with conditions

        public OSqlTraverse Where(string field)
        {
            _sqlQuery.Where(field);

            return this;
        }

        public OSqlTraverse And(string field)
        {
            _sqlQuery.And(field);

            return this;
        }

        public OSqlTraverse Or(string field)
        {
            _sqlQuery.Or(field);

            return this;
        }

        public OSqlTraverse Equals<T>(T item)
        {
            _sqlQuery.Equals<T>(item);

            return this;
        }

        public OSqlTraverse NotEquals<T>(T item)
        {
            _sqlQuery.NotEquals<T>(item);

            return this;
        }

        public OSqlTraverse Lesser<T>(T item)
        {
            _sqlQuery.Lesser<T>(item);

            return this;
        }

        public OSqlTraverse LesserEqual<T>(T item)
        {
            _sqlQuery.LesserEqual<T>(item);

            return this;
        }

        public OSqlTraverse Greater<T>(T item)
        {
            _sqlQuery.Greater<T>(item);

            return this;
        }

        public OSqlTraverse GreaterEqual<T>(T item)
        {
            _sqlQuery.GreaterEqual<T>(item);

            return this;
        }

        public OSqlTraverse Like<T>(T item)
        {
            _sqlQuery.Like<T>(item);

            return this;
        }

        public OSqlTraverse IsNull()
        {
            _sqlQuery.IsNull();

            return this;
        }

        public OSqlTraverse Contains<T>(T item)
        {
            _sqlQuery.Contains<T>(item);

            return this;
        }

        public OSqlTraverse Contains<T>(string field, T value)
        {
            _sqlQuery.Contains<T>(field, value);

            return this;
        }

        #endregion

        public OSqlTraverse OrderBy(params string[] fields)
        {
            _sqlQuery.OrderBy(fields);

            return this;
        }

        public OSqlTraverse Ascending()
        {
            _sqlQuery.Ascending();

            return this;
        }

        public OSqlTraverse Descending()
        {
            _sqlQuery.Descending();

            return this;
        }

        public OSqlTraverse Skip(int skipCount)
        {
            _sqlQuery.Skip(skipCount);

            return this;
        }

        public OSqlTraverse Limit(int maxRecords)
        {
            _sqlQuery.Limit(maxRecords);

            return this;
        }

        #region ToList

        public List<T> ToList<T>() where T : class, new()
        {
            List<T> result = new List<T>();
            List<ODocument> documents = ToList("*:0");

            foreach (ODocument document in documents)
            {
                result.Add(document.To<T>());
            }

            return result;
        }

        public List<ODocument> ToList()
        {
            return ToList("*:0");
        }

        public List<ODocument> ToList(string fetchPlan)
        {
            CommandPayload payload = new CommandPayload();
            payload.Type = CommandPayloadType.Sql;
            payload.Text = ToString();
            payload.NonTextLimit = -1;
            payload.FetchPlan = fetchPlan;
            payload.SerializedParams = new byte[] { 0 };

            Command operation = new Command();
            operation.OperationMode = OperationMode.Asynchronous;
            operation.ClassType = CommandClassType.Idempotent;
            operation.CommandPayload = payload;

            OCommandResult commandResult = new OCommandResult(_connection.ExecuteOperation(operation));

            return commandResult.ToList();
        }

        #endregion

        public override string ToString()
        {
            return _sqlQuery.ToString(QueryType.Traverse);
        }
    }
}
