using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHS.Sage.Mapping;
using System.Collections.ObjectModel;

namespace SHS.Sage.Linq.Mapping
{
    public class OMappingEntity<T> : OMappingEntity where T : IIdentifiable
    {
        public OMappingEntity(Type repositoryType, params OMappingProperty[] properties) : base(typeof(T), repositoryType, properties) { }
    }

    public class OMappingEntity : MappingEntity
    {
        string _storageClass;
        Type _type;
        ReadOnlyCollection<IMappedProperty> _properties;
        Type _repoType;

        public OMappingEntity(Type type, Type repositoryType, params OMappingProperty[] properties)
        {
            _type = type;
            _storageClass = type.Equals(typeof(IIdentifiable)) || type.Equals(typeof(_Thing)) 
                ? "V" 
                : type.Equals(typeof(_Association)) ? "E" : type.Name;
            _properties = properties.OfType<IMappedProperty>().ToReadOnly();
            _repoType = repositoryType;
        }

        public override string StorageClass
        {
            get { return _storageClass; }
            set { _storageClass = value; }
        }

        public override Type EntityType
        {
            get { return _type; }
        }

        public override IEnumerable<IMappedProperty> Properties
        {
            get
            {
                return _properties;
            }
        }

        public override Type RepositoryType
        {
            get
            {
                return _repoType;
            }
        }
    }
}
