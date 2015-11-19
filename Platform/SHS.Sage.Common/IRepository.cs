using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IRepository : IDisposable, IExecuteQueries
    {
        /// <summary>
        /// Gets a boolean value indicating whether the IRepository instance is valid for use.
        /// </summary>
        bool IsValid { get; }
        /// <summary>
        /// Creates an queryable instance of Thing types specified by T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IThingSet<T> ThingSet<T>() where T : IThing;
        /// <summary>
        /// Creates an queryable instance of Association types specified by T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IAssociationSet<T> AssociationSet<T>() where T : IAssociation;
        /// <summary>
        /// Creates an queryable instance of Activity types specified by T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IActivitySet<T> ActivitySet<T>() where T : IActivity;
        /// <summary>
        /// Gets a populated Thing instance for the provided activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        T Get<T>(T identity) where T : IIdentifiable;
        /// <summary>
        /// Gets a populated IIdentifiable instance with the specified IIdentity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        IIdentifiable Get(IIdentifiable identity);
        /// <summary>
        /// Gets a populated IIdentifiable instance by the specified Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IIdentifiable Get(string id);
        /// <summary>
        /// Gets a typed identifiable by its Id value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(string id) where T: IIdentifiable;
        /// <summary>
        /// Gets the policy to use for queries on this repository instance
        /// </summary>
        IQueryPolicy Policy { get; }
        /// <summary>
        /// Gets an instance of the query cache
        /// </summary>
        ICacheQueries QueryCache { get; }
        /// <summary>
        /// Gets an instance of the entity mapping system
        /// </summary>
        IMapEntities Mapping { get; }
    }
}
