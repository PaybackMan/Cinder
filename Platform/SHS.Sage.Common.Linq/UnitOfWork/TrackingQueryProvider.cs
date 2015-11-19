using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.UnitOfWork
{
    public class TrackingQueryProvider : DataProvider, IExecuteQueries
    {
        public TrackingQueryProvider(DataProvider provider) 
            : base(provider.Repository, provider.Language, provider.Mapping, provider.Policy)
        {
            this.Provider = provider;
            this.Repository = provider.Repository;
        }

        public DataProvider Provider { get; private set; }

        public IEnumerable<T> ExecuteEnumerable<T>(string query) where T : IIdentifiable
        {
            return this.CreateExecutor().ExecuteEnumerable<T>(query);
        }

        public IReadData ExecuteReader(string query)
        {
            return this.CreateExecutor().ExecuteReader(query);
        }

        public void ExecuteStatement(string query)
        {
            this.CreateExecutor().ExecuteStatement(query);
        }

        protected override QueryExecutor CreateExecutor()
        {
            return new TrackingQueryExecutor((ITrackingRepository)this.Repository, ((ICreateExecutor)this.Provider).CreateExecutor());
        }
    }
}
