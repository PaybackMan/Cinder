using SHS.Sage.Data;
using SHS.Sage.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class OQueryExecutor : QueryExecutor
    {
        OQueryProvider _provider;
        public OQueryExecutor(OQueryProvider provider)
        {
            _provider = provider;
        }

        public override int RowsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public override object Convert(object value, Type type)
        {
            if (value == null)
            {
                return TypeHelper.GetDefault(type);
            }
            type = TypeHelper.GetNonNullableType(type);
            Type vtype = value.GetType();
            if (type != vtype)
            {
                if (type.IsEnum)
                {
                    if (vtype == typeof(string))
                    {
                        return Enum.Parse(type, (string)value);
                    }
                    else
                    {
                        Type utype = Enum.GetUnderlyingType(type);
                        if (utype != vtype)
                        {
                            value = System.Convert.ChangeType(value, utype);
                        }
                        return Enum.ToObject(type, value);
                    }
                }

                if (type.Implements(typeof(IIdentifiable))
                    && (value is Orient.Client.ORID))
                {
                    return CreateComplexEntity(value, type);
                }

                if (value is Orient.Client.ORID
                    && type.Equals(typeof(string)))
                {
                    return value.ToString();
                }

                if (TypeHelper.IsEnumerableOrArray(type))
                {
                    var aggregator = SHS.Sage.Linq.Expressions.Aggregator.GetAggregator(type, vtype).Compile();
                    return aggregator.DynamicInvoke(value);
                }

                return System.Convert.ChangeType(value, type);
            }
            return value;
        }

        protected virtual object CreateComplexEntity(object value, Type type)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues)
        {
            try
            {
                DbCommand cmd = this.GetCommand(command, paramValues);
                DbDataReader reader = cmd.ExecuteReader();
                var result = Project(reader, fnProjector, entity, true);
                return new EnumerateOnce<T>(result);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Handles class for proxied types so we can plug in a current Repository instance allowing deferred loading 
        /// for complex types and collections
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="fnProjector"></param>
        /// <param name="entity"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public override IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, IRepository, T> fnProjector, MappingEntity entity, object[] paramValues)
        {
            try
            {
                DbCommand cmd = this.GetCommand(command, paramValues);
                DbDataReader reader = cmd.ExecuteReader();
                var result = Project(reader, fnProjector, entity, true);
                return new EnumerateOnce<T>(result); ;
            }
            finally
            {
            }
        }

        protected virtual IEnumerable<T> Project<T>(DbDataReader reader, Func<FieldReader, T> fnProjector, MappingEntity entity, bool closeReader)
        {
            var freader = new OFieldReader(this, reader);
            try
            {
                while (reader.Read())
                {
                    yield return fnProjector(freader);
                }
            }
            finally
            {
                if (closeReader)
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// called internally to enable passing the current repository instance into the projector.  used by proxy types 
        /// to determine if repo is still alive prior to running deferred load calls.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="fnProjector"></param>
        /// <param name="entity"></param>
        /// <param name="closeReader"></param>
        /// <returns></returns>
        protected virtual IEnumerable<T> Project<T>(DbDataReader reader, Func<FieldReader, IRepository, T> fnProjector, MappingEntity entity, bool closeReader)
        {
            var freader = new OFieldReader(this, reader);
            try
            {
                var list = new List<T>();
                while (reader.Read())
                {
                   list.Add(fnProjector(freader, this._provider.Repository));
                }

                // because our orient queries share a single network stream, 
                // we need to pre-read all the items in our enumerable before 
                // breaking out of here in order to ensure that any deferred 
                // load sub-properties don't overwrite our network stream 
                // before we have finished enumerating
                foreach(var item in list)
                {
                    yield return item;
                }
            }
            finally
            {
                if (closeReader)
                {
                    reader.Close();
                }
            }
        }

        protected virtual DbCommand GetCommand(QueryCommand query, object[] paramValues)
        {
            var cmd = this._provider.CreateCommand(query);
            cmd.CommandText = query.CommandText;
            // handle transactions here
            this.SetParameters(cmd, query, paramValues);
            return cmd;
        }

        protected virtual void SetParameters(DbCommand command, QueryCommand query, object[] paramValues)
        {
            if (query.Parameters.Count > 0 && command.Parameters.Count == 0)
            {
                for (int i = 0, n = query.Parameters.Count; i < n; i++)
                {
                    this.AddParameter(command, query.Parameters[i], paramValues != null ? paramValues[i] : null);
                }
            }
            else if (paramValues != null)
            {
                for (int i = 0, n = command.Parameters.Count; i < n; i++)
                {
                    DbParameter p = command.Parameters[i];
                    if (p.Direction == System.Data.ParameterDirection.Input
                     || p.Direction == System.Data.ParameterDirection.InputOutput)
                    {
                        p.Value = paramValues[i] ?? DBNull.Value;
                    }
                }
            }
        }

        protected virtual void AddParameter(DbCommand command, QueryParameter parameter, object value)
        {
            DbParameter p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.Value = value ?? DBNull.Value;
            ((ODataParameter)p).DestinationType = parameter.Type;
            command.Parameters.Add(p);
        }

        public override int ExecuteCommand(QueryCommand command, object[] paramValues)
        {
            command.IsIdempotent = false;
            var queryCommand = GetCommand(command, paramValues);
            var result = queryCommand.ExecuteNonQuery();
            return result;
        }

        public override void ExecuteStatement(string query)
        {
            string[] statements = query.Split(';');
            foreach (var statement in statements)
            {
                if (string.IsNullOrEmpty(statement)) continue;

                var isIdempotent = statement.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase);
                var queryCommand = GetCommand(
                    new QueryCommand(statement, new QueryParameter[0])
                    {
                        IsIdempotent = isIdempotent
                    },
                    new object[0]);
                queryCommand.ExecuteNonQuery();
            }
        }

        public override IReadData ExecuteReader(string query)
        {
            var isIdempotent = query.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase);
            var queryCommand = GetCommand(
                new QueryCommand(query, new QueryParameter[0])
                {
                    IsIdempotent = isIdempotent
                },
                new object[0]);
            return (IReadData)queryCommand.ExecuteReader();
        }

        public override IEnumerable<T> ExecuteEnumerable<T>(string query)
        {
            var isIdempotent = query.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase);
            var queryCommand = GetCommand(
                new QueryCommand(query, new QueryParameter[0])
                {
                    IsIdempotent = isIdempotent
                },
                new object[0]);

            using (var reader = queryCommand.ExecuteReader())
            {
                var typeBuilder = new OTypeBuilder(this._provider.Language.CreateLinguist(null));
                var freader = new OFieldReader(this, reader);
                while (reader.Read())
                {
                    yield return typeBuilder.BuildDeferredType<T>(freader, this._provider.Repository);
                }
            }
        }
    }
}
