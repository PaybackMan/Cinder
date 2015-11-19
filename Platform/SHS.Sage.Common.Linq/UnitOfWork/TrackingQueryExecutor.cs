using SHS.Sage.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.UnitOfWork
{
    /// <summary>
    /// Query executor that contains a non-tracking executor, and wraps the returned entity instances 
    /// attaching them to the current ITrackingRepository instance for which the queries are called.
    /// </summary>
    public class TrackingQueryExecutor : QueryExecutor
    {
        public TrackingQueryExecutor(ITrackingRepository repository, QueryExecutor executor)
        {
            this.Repository = repository;
            this.Executor = executor;
        }

        public QueryExecutor Executor { get; private set; }
        public ITrackingRepository Repository { get; private set; }

        public override object Convert(object value, Type type)
        {
            return this.Executor.Convert(value, type);
        }

        public override int ExecuteCommand(QueryCommand query, object[] paramValues)
        {
            return this.Executor.ExecuteCommand(query, paramValues);
        }

        public override IEnumerable<T> ExecuteEnumerable<T>(string query)
        {
            return this.Executor.ExecuteEnumerable<T>(query);
        }

        public override IReadData ExecuteReader(string query)
        {
            return this.Executor.ExecuteReader(query);
        }

        public override void ExecuteStatement(string query)
        {
            this.Executor.ExecuteStatement(query);
        }

        public override int RowsAffected
        {
            get
            {
                return this.Executor.RowsAffected;
            }
        }

        public override IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, IRepository, T> fnProjector, MappingEntity entity, object[] paramValues)
        {
            return new TrackingEnumerator<T>(this.Executor.Execute<T>(command, Wrap(fnProjector), entity, paramValues), this.Repository);
        }

        public override IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues)
        {
            return new TrackingEnumerator<T>(this.Executor.Execute<T>(command, Wrap(fnProjector), entity, paramValues), this.Repository);
        }

        /// <summary>
        /// Called during execution to attach the returned entity instance to the current tracking repository if the Policy.TrackChanges is enabled
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        protected virtual Func<FieldReader, T> Wrap<T>(Func<FieldReader, T> projector)
        {
            if (this.Repository.Policy.TrackChanges)
            {
                Func<FieldReader, T> wrapper = (reader) => (T)this.Repository.Attach<T>(projector(reader), TrackingState.Unknown);
                return wrapper;
            }
            else
            {
                return projector;
            }
        }

        /// <summary>
        /// Called during execution to attach the returned entity instance to the current tracking repository if the Policy.TrackChanges is enabled
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projector"></param>
        /// <returns></returns>
        protected virtual Func<FieldReader, IRepository, T> Wrap<T>(Func<FieldReader, IRepository, T> projector)
        {
            if (this.Repository.Policy.TrackChanges)
            {
                Func<FieldReader, IRepository, T> wrapper = (reader, repo) => (T)this.Repository.Attach<T>(projector(reader, repo), TrackingState.Unknown);
                return wrapper;
            }
            else
            {
                return projector;
            }
        }
    }
}
