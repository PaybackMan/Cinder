using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Mapping
{
    public interface IFluentMapper
    {
        /// <summary>
        /// Returns true if a mapping for the specified type already exists
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsMapped(Type type);
        /// <summary>
        /// Returns true if a mapping for the specified type already exists
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        bool IsMapped<TEntity>() where TEntity : IIdentifiable;
        /// <summary>
        /// Gets an existing mapping for the specified type, or returns null
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IFluentEntityMapper<TEntity> GetMapping<TEntity>() where TEntity : IIdentifiable;
        /// <summary>
        /// Creates (or updates) a mapping for the specified entity type
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IFluentEntityMapper<TEntity> MapEntity<TEntity>() where TEntity : IIdentifiable;
        /// <summary>
        /// Creates and returns the mapping entity entries for the currently mapped types
        /// </summary>
        IEnumerable<IMappedEntity> Entities { get; }
        /// <summary>
        /// Creates a mapping entry from a type of IIdentifiable using default settings
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IMappedEntity AutoMap(Type type);
    }

    public interface IFluentEntityMapper<TEntity> where TEntity : IIdentifiable
    {
        /// <summary>
        /// Set the mapped name of the underlying storage object for the entity
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFluentEntityMapper<TEntity> StorageName(Expression<Func<string>> name);
        /// <summary>
        /// Maps all public properties on the entity using same-name matching, and sets the StorageName to the type name
        /// </summary>
        /// <returns></returns>
        IFluentMapper DefaultMappings();
        /// <summary>
        /// Maps an individual property to the storage provider
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        IFluentPropertyMapper<TEntity> MapProperty(Expression<Func<TEntity, object>> property);
        /// <summary>
        /// Enumerates the list of mapped properties
        /// </summary>
        IEnumerable<IMappedProperty> Properties { get; }
        /// <summary>
        /// Gets the mapper instance
        /// </summary>
        IFluentMapper Mapper { get; }
    }

    public interface IFluentPropertyMapper<TEntity> where TEntity : IIdentifiable
    {
        /// <summary>
        /// Sets the mapped property name on the storage provider
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFluentPropertyMapper<TEntity> StorageName(Expression<Func<string>> name);
        /// <summary>
        /// Maps an individual property to the storage provider
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        IFluentPropertyMapper<TEntity> MapProperty(Expression<Func<TEntity, object>> property);
        /// <summary>
        /// Gets the mapped entity instance
        /// </summary>
        IFluentEntityMapper<TEntity> Entity { get; }
    }

    public interface IFluentEntityMapperConverter
    {
        IMappedEntity ToMappedEntity();
    }

    public interface IFluentPropertyMapperConverter
    {
        IMappedProperty ToMappedProperty();
    }
}
