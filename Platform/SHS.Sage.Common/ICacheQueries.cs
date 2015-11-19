using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface ICacheQueries
    {
        /// <summary>
        /// Compiles, caches and executes the supplied expression, if possible
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        object Execute(Expression query);
        /// <summary>
        /// Compiles, caches and executes the supplied query, if possible
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        object Execute(IQueryable query);
        /// <summary>
        /// Compiles, caches and executes the supplied query, if possible
        /// </summary>
        /// <typeparam name="T">the query result type</typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<T> Execute<T>(IQueryable<T> query);
        /// <summary>
        /// Gets the number of entries in the cache
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Deletes all cached entries
        /// </summary>
        void Clear();
        /// <summary>
        /// Gets a boolean value to determine if the provided query is already cached
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        bool Contains(Expression query);
        /// <summary>
        /// Gets a boolean value to determine if the provided query is already cached
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        bool Contains(IQueryable query);
        /// <summary>
        /// Get/set maximum number of cache entries to store
        /// </summary>
        int MaxDepth { get; set; }
    }
}
