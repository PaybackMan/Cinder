
using System.Collections.Generic;

namespace SHS.Sage
{
    public interface IExecuteQueries
    {
        /// <summary>
        /// Execute a raw query statement directly against the store, without returning any results
        /// </summary>
        /// <param name="query">a structured query in the laguage of the underlying provider</param>
        void ExecuteStatement(string query);
        /// <summary>
        /// Execute a raw query statement directly against the store, returning an IReadData instance 
        /// allowing for enumerating results of the query.
        /// </summary>
        /// <param name="query">a structured query in the laguage of the underlying provider</param>
        /// <returns></returns>
        IReadData ExecuteReader(string query);
        /// <summary>
        /// Execute a raw query statement firectly against the store, and marshal the results into an 
        /// enumerable of type T
        /// </summary>
        /// <typeparam name="T">the identifiable type to marshal results into</typeparam>
        /// <param name="query">a structured query in the laguage of the underlying provider</param>
        /// <returns></returns>
        IEnumerable<T> ExecuteEnumerable<T>(string query) where T : IIdentifiable;
    }
}
