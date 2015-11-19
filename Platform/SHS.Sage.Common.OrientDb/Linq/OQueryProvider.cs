using SHS.Sage.Data;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Mapping;
using SHS.Sage.Linq.Policy;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class OQueryProvider : DataProvider, IExecuteQueries
    { 
        public OQueryProvider(ORepository repository)
            : base(repository, new OQueryLanguage(), OTypeMapping.Mappings(repository), repository.Policy as QueryPolicy)
        {
            this.Repository = repository;
            this.Cache = repository.QueryCache;
        }

        protected override QueryExecutor CreateExecutor()
        {
            return new OQueryExecutor(this);
        }

        public override IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                if (elementType.Implements(typeof(IThing)))
                {
                    return (IQueryable)Activator.CreateInstance(typeof(OThingSet<>).MakeGenericType(elementType), new object[] { this, this.Repository, expression });
                }
                else if (elementType.Implements(typeof(IAssociation)))
                {
                    return (IQueryable)Activator.CreateInstance(typeof(OAssociationSet<>).MakeGenericType(elementType), new object[] { this, this.Repository, expression });
                }
                else if (elementType.Implements(typeof(IActivity)))
                {
                    return (IQueryable)Activator.CreateInstance(typeof(OActivitySet<>).MakeGenericType(elementType), new object[] { this, this.Repository, expression });
                }
                else return base.CreateQuery(expression);
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        internal virtual DbCommand CreateCommand(QueryCommand query)
        {
            return new ODataCommand(((ORepository)Repository).Connection, query.CommandText, query.IsIdempotent);
        }

        public virtual void ExecuteStatement(string query)
        {
            new OQueryExecutor(this).ExecuteStatement(query);
        }

        public virtual IReadData ExecuteReader(string query)
        {
            return new OQueryExecutor(this).ExecuteReader(query);
        }

        public virtual IEnumerable<T> ExecuteEnumerable<T>(string query) where T : IIdentifiable
        {
            return new OQueryExecutor(this).ExecuteEnumerable<T>(query);
        }
    }
}
