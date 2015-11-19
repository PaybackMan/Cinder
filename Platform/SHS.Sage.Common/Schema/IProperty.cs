using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Schema
{
    
    public interface IProperty
    {
        /// <summary>
        /// The name of the property in the storage provider
        /// </summary>
        string Name { get; }
        /// <summary>
        /// True if the property is mandatory
        /// </summary>
        bool IsRequired { get; }
        /// <summary>
        /// True if the property cannot be null
        /// </summary>
        bool IsNotNull { get; }
        /// <summary>
        /// True if the property cannot be written to
        /// </summary>
        bool IsReadOnly { get; }
        /// <summary>
        /// A numerical value used to determine the property's underlying storage type
        /// </summary>
        int PropertyType { get; }
        /// <summary>
        /// Returns a string value for the property's data type
        /// </summary>
        /// <returns></returns>
        string PropertyTypeText();
    }
}
