using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Mapping
{
    public interface IMappedEntity
    {
        /// <summary>
        /// The name of the class in the storage provider
        /// </summary>
        string StorageClass { get; }
        /// <summary>
        /// The .Net type being mapped
        /// </summary>
        Type EntityType { get; }
        /// <summary>
        /// The mapped properties
        /// </summary>
        IEnumerable<IMappedProperty> Properties { get; }
        /// <summary>
        /// The type of repository that this entity is mapped for
        /// </summary>
        Type RepositoryType { get; }
    }
}
