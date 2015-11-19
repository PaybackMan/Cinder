using SHS.Sage.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public abstract class QueryExecutor : IExecuteQueries
    {
        // called from compiled execution plan
        public abstract int RowsAffected { get; }
        public abstract object Convert(object value, Type type);
        public abstract IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues);
        public abstract IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, IRepository, T> fnProjector, MappingEntity entity, object[] paramValues);
        public abstract int ExecuteCommand(QueryCommand query, object[] paramValues);
        public abstract IEnumerable<T> ExecuteEnumerable<T>(string query) where T : IIdentifiable;
        public abstract IReadData ExecuteReader(string query);
        public abstract void ExecuteStatement(string query);
    }
}
