using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using SHS.Sage.Schema;

namespace SHS.Sage.Linq.Mapping
{
    public class OFluentMapper : IFluentMapper
    {
        public OFluentMapper(Type repositoryType)
        {
            this.RepositoryType = repositoryType;
        }

        private List<IFluentEntityMapperConverter> _entities = new List<IFluentEntityMapperConverter>();
        public IEnumerable<IMappedEntity> Entities
        {
            get
            {
                foreach(var entity in _entities)
                {
                    yield return entity.ToMappedEntity();
                }
            }
        }

        public Type RepositoryType { get; private set; }

        public IFluentEntityMapper<TEntity> MapEntity<TEntity>() where TEntity : IIdentifiable
        {
            var entityMapper = new OFluentEntityMapper<TEntity>(this);
            _entities.Add(entityMapper);
            return entityMapper;
        }

        public bool IsMapped<TEntity>() where TEntity : IIdentifiable
        {
            return IsMapped(typeof(TEntity));
        }

        public bool IsMapped(Type entityType)
        {
            return _entities.Any(e => e.GetType().GetGenericArguments()[0].Equals(entityType));
        }

        public IFluentEntityMapper<TEntity> GetMapping<TEntity>() where TEntity : IIdentifiable
        {
            return (IFluentEntityMapper<TEntity>)_entities
                .SingleOrDefault(e => e.GetType().GetGenericArguments()[0].Equals(typeof(TEntity)));
        }

        public IMappedEntity AutoMap(Type type)
        {
            var fluentMapperParam = Expression.Parameter(this.GetType());
            var mapEntityCall = Expression.Call(fluentMapperParam, this.GetType().GetMethod("MapEntity").MakeGenericMethod(type));
            
            var fluentEntityType = typeof(OFluentEntityMapper<>).MakeGenericType(type);
            var entityVar = Expression.Variable(fluentEntityType, "entityMapper");
            var entityVarAssign = Expression.Assign(entityVar, Expression.Convert(mapEntityCall, fluentEntityType));

            var defaultMappingsCall = Expression.Call(entityVar, fluentEntityType.GetMethod("DefaultMappings"));
            var toMappedEntityCall = Expression.Call(entityVar, fluentEntityType.GetMethod("ToMappedEntity"));

            var returnTarget = Expression.Label(typeof(IMappedEntity));

            var blockBody = Expression.Block(typeof(IMappedEntity), 
                new ParameterExpression[] { entityVar }, 
                entityVarAssign, defaultMappingsCall, Expression.Label(returnTarget, toMappedEntityCall));

            return Expression.Lambda<Func<OFluentMapper, IMappedEntity>>(blockBody, fluentMapperParam).Compile()(this);
        }
    }

    public class OFluentEntityMapper<TEntity> : IFluentEntityMapper<TEntity>, IFluentEntityMapperConverter where TEntity : IIdentifiable
    {
        private readonly OFluentMapper _mapper;
        private Dictionary<string, IFluentPropertyMapper<TEntity>> _properties = new Dictionary<string, IFluentPropertyMapper<TEntity>>();
        private Expression<Func<string>> _storageName;
        private Func<string> _storageNameFunc;
        private bool _canSetStorageName = true;
        public OFluentEntityMapper(OFluentMapper mapper)
        {
            this._mapper = mapper;

            if (typeof(TEntity).Implements<IAssociation>())
            {
                ClassType = ClassType.Association;
            }
            else if (typeof(TEntity).Implements<IThing>()
                || typeof(TEntity).Implements<IIdentifiable>())
            {
                ClassType = ClassType.Thing;
            }
            else
            {
                throw new NotSupportedException("Mapped type must implement IAssociation or IThing");
            }
            
            if (typeof(TEntity).Equals(typeof(_Actor))
                || typeof(TEntity).Equals(typeof(IActor)) 
                || typeof(TEntity).Equals(typeof(_Thing))
                || typeof(TEntity).Equals(typeof(IThing))
                || typeof(TEntity).Equals(typeof(IIdentifiable)))
            {
                StorageName(() => "V");
                _canSetStorageName = false;
            }
            else if (typeof(TEntity).Equals(typeof(_Association))
                || typeof(TEntity).Equals(typeof(IAssociation)))
            {
                StorageName(() => "E");
                _canSetStorageName = false;
            }
            else
            {
                StorageName(() => typeof(TEntity).Name);
            }
        }

        public IEnumerable<IMappedProperty> Properties
        {
            get
            {
                if (!_properties.ContainsKey("Id"))
                {
                    var pi = typeof(TEntity).GetPublicProperty("Id");
                    CreatePropertyMapping(pi);
                }

                if (this.ClassType == ClassType.Association)
                {
                    if (!_properties.ContainsKey("Source"))
                    {
                        var pi = typeof(TEntity).GetPublicProperty("Source");
                        CreatePropertyMapping(pi);
                    }
                    if (!_properties.ContainsKey("Target"))
                    {
                        var pi = typeof(TEntity).GetPublicProperty("Target");
                        CreatePropertyMapping(pi);
                    }
                }

                foreach(var prop in _properties.Values)
                {
                    yield return ((OFluentPropertyMapper<TEntity>)prop).ToMappedProperty();
                }
            }
        }

        public IFluentMapper DefaultMappings()
        {
            foreach(var pi in typeof(TEntity)
                .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite && p.CanRead))
            {
                CreatePropertyMapping(pi);
            }

            return this._mapper;
        }

        private void CreatePropertyMapping(PropertyInfo pi)
        {
            var entityParam = Expression.Parameter(typeof(TEntity));
            var memberAccess = Expression.Convert(Expression.MakeMemberAccess(entityParam, pi), typeof(object));
            var propertyExpression = Expression.Lambda<Func<TEntity, object>>(memberAccess, entityParam);
            MapProperty(propertyExpression);
        }

        public IFluentPropertyMapper<TEntity> MapProperty(Expression<Func<TEntity, object>> property)
        {
            if (!property.Contains<MemberExpression>())
                throw new InvalidOperationException("Property mappings can only reference public instance properties");

            var fluentProperty = new OFluentPropertyMapper<TEntity>(this, property);
            if (this._properties.ContainsKey(fluentProperty.PropertyMember.Name))
            {
                this._properties[fluentProperty.PropertyMember.Name] = fluentProperty;
            }
            else
            {
                this._properties.Add(fluentProperty.PropertyMember.Name, fluentProperty);
            }
            return fluentProperty;
        }

        public IFluentEntityMapper<TEntity> StorageName(Expression<Func<string>> name)
        {
            var compiled = name.Compile();

            if (!_canSetStorageName && !compiled().Equals(_storageNameFunc()))
                throw new InvalidOperationException("The storage provider name cannot be set for this type.");

            this._storageName = name;
            this._storageNameFunc = name.Compile();

            return this;
        }

        public IFluentMapper Mapper {  get { return _mapper; } }

        public IMappedEntity ToMappedEntity()
        {
            var entity = new OMappingEntity(typeof(TEntity), this._mapper.RepositoryType, this.Properties.OfType<OMappingProperty>().ToArray())
            {
                StorageClass = _storageNameFunc()
            };
            return entity;
        }

        public ClassType ClassType { get; private set; }
    }

    public class OFluentPropertyMapper<TEntity> : IFluentPropertyMapper<TEntity>, IFluentPropertyMapperConverter where TEntity : IIdentifiable
    {
        private readonly OFluentEntityMapper<TEntity> _entityMapper;
        private Expression<Func<string>> _storageName;
        private Func<string> _storageNameFunc;
        private readonly Expression<Func<TEntity, object>> _property;

        public OFluentPropertyMapper(OFluentEntityMapper<TEntity> entityMapper, Expression<Func<TEntity, object>> property)
        {
            this._entityMapper = entityMapper;
            this._property = property;
            this.PropertyMember = ExpressionFinder<MemberExpression>.FindFirst(this._property).Member as PropertyInfo;
            if (entityMapper.ClassType == ClassType.Thing)
            {
                StorageName(() => PropertyMember.Name);
            }
            else if(entityMapper.ClassType == ClassType.Association)
            {
                if (this.PropertyMember.Name.Equals("Source"))
                {
                    StorageName(() => "out");
                }
                else if (this.PropertyMember.Name.Equals("Target"))
                {
                    StorageName(() => "in");
                }
                else
                {
                    StorageName(() => PropertyMember.Name);
                }
            }
            else
            {
                throw new NotSupportedException("Mapped type must implement IAssociation or IThing");
            }
        }

        public PropertyInfo PropertyMember { get; private set; }

        public IFluentPropertyMapper<TEntity> MapProperty(Expression<Func<TEntity, object>> property)
        {
            return this._entityMapper.MapProperty(property);
        }

        public IFluentPropertyMapper<TEntity> StorageName(Expression<Func<string>> name)
        {
            var compiled = name.Compile();

            if (this.PropertyMember.Name.Equals("Id") && !compiled().Equals("Id"))
                throw new InvalidOperationException("The Id property cannot be renamed in the storage provider.");

            if (this._entityMapper.ClassType == ClassType.Association)
            {
                if (this.PropertyMember.Name.Equals("Source") && !compiled().Equals("out"))
                {
                    throw new InvalidOperationException("The Source property cannot be renamed in the storage provider.");
                }
                else if (this.PropertyMember.Name.Equals("Target") && !compiled().Equals("in"))
                {
                    throw new InvalidOperationException("The Target property cannot be renamed in the storage provider.");
                }
            }

            this._storageName = name;
            this._storageNameFunc = compiled;
            return this;
        }

        public IFluentEntityMapper<TEntity> Entity { get { return _entityMapper; } }

        public IMappedProperty ToMappedProperty()
        {
            var property = new OMappingProperty(PropertyMember, this._storageNameFunc());
            return property;
        }
    }
}
