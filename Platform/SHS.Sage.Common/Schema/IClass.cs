using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Schema
{
    public enum ClassType
    {
        Unknown = 0,
        Thing,
        Association,
        Activity,
        Entitlement
    }

    public interface IClass<TRepository> : IClass
        where TRepository : IRepository
    {
    }
    public interface IClass
    { 
        /// <summary>
        /// True if the class is abstract
        /// </summary>
        bool IsAbstract { get; }
        /// <summary>
        /// The class' name in the storage system
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Defines the matching pattern to use to determine if an Id value applies to this class
        /// </summary>
        string IdMask { get; }
        /// <summary>
        /// Tests a given Id against the IdMask to determine if the supplied Id matches this class
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsMaskedId(string id);
        /// <summary>
        /// Enumerates the list of properties for this class
        /// </summary>
        IEnumerable<IProperty> Properties { get; }
        /// <summary>
        /// Adds a property to the class
        /// </summary>
        /// <param name="property"></param>
        void AddProperty(IProperty property);
        /// <summary>
        /// Removes a property from the class
        /// </summary>
        /// <param name="property"></param>
        void RemoveProperty(IProperty property);
        /// <summary>
        /// Alters a property on the class
        /// </summary>
        /// <param name="property"></param>
        void AlterProperty(IProperty property);
        /// <summary>
        /// The base type name from which this class derives
        /// </summary>
        string BaseClass { get; }
        /// <summary>
        /// The class' object type
        /// </summary>
        ClassType ClassType { get; }
    }
}
