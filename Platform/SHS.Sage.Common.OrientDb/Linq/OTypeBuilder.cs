using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using SHS.Sage.Linq.Runtime;
using SHS.Sage.Linq.Expressions;
using System.Reflection;
using System.Threading;
using SHS.Sage.Linq.Mapping;
using SHS.Sage.Mapping;

namespace SHS.Sage.Linq
{
    public class OTypeBuilder
    {
        public OTypeBuilder(QueryLinguist linguist)
        {
            this.Linguist = linguist;
        }

        public QueryLinguist Linguist { get; private set; }

        public T BuildNonDeferredType<T>(FieldReader reader, IRepository repository) where T : IIdentifiable
        {
            CachedInitializer<Func<FieldReader, object>> entry =
                GetInitializer<Func<FieldReader, object>>(
                    _nativeInitializers,
                    reader,
                    repository);

            return (T)entry.Initializer(reader);
        }

        public T BuildDeferredType<T>(FieldReader reader, IRepository repository) where T : IIdentifiable
        {
            CachedInitializer<Func<FieldReader, IRepository, object>> entry = 
                GetInitializer<Func<FieldReader, IRepository, object>>(
                    _proxiedInitializers, 
                    reader,
                    repository);

            return (T)entry.Initializer(reader, repository);
        }

        private CachedInitializer<T> GetInitializer<T>(Dictionary<string, CachedInitializer<T>> source, FieldReader reader, IRepository repository)
        {
            _lock.EnterUpgradeableReadLock();
            CachedInitializer<T> entry;
            try
            {
                var className = reader.ReadValue<string>("+class");

                if (string.IsNullOrEmpty(className)
                    && typeof(T).Implements<IIdentifiable>())
                {
                    className = GetMapping(typeof(T), repository).StorageClass;
                }
                else 
                {
                    var id = reader.ReadString("Id");
                    if (!string.IsNullOrEmpty(id))
                    {
                        className = GetMappingFromId(id, repository).StorageClass;
                    }

                    if (string.IsNullOrEmpty(className))
                        throw new NotSupportedException(string.Format("Cannot create initializer for type {0}.", typeof(T).Name));
                }

                if (!source.TryGetValue(className, out entry))
                {
                    var type = GetMapping(className, repository);
                    GetDefaultInitializer(type, false, OTypeMapping.Mappings(repository));
                    entry = source[type.EntityType.Name];
                }

                return entry;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IMappedEntity GetMappingFromId(string id, IRepository repository)
        {
            return OTypeMapping.Mappings(repository).GetEntity(id, repository.GetType());
        }

        public IMappedEntity GetMapping(string className, IRepository repository)
        {
            return OTypeMapping.Mappings(repository).FirstOrDefault(me => me.StorageClass.Equals(className));
        }

        public IMappedEntity GetMapping(Type type, IRepository repository)
        {
            return OTypeMapping.Mappings(repository).FirstOrDefault(me => me.EntityType.Equals(type));
        }

        public MemberInitExpression GetDefaultInitializer(IMappedEntity type, bool deferLoadComplexTypes, IMapEntities mapping)
        { 
            var bindings = GetDefaultBindings(type).ToArray();
            var reader = ((MethodCallExpression)((MemberAssignment)bindings[0]).Expression).Object as ParameterExpression;

            MemberInitExpression init;
            if (deferLoadComplexTypes)
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    CachedInitializer<Func<FieldReader, IRepository, object>> entry;
                    if (!_proxiedInitializers.TryGetValue(type.EntityType.Name, out entry))
                    {
                        // creates a new proxy type to lazy load complex property types
                        var proxyType = IdentifiableProxyBuilder.GetProxyType(type.EntityType);
                        // has a ctor that takes two arguments IdentifiableProxy(IRepository repository, ProxiedIdBag idBag)
                        var proxyCtor = proxyType.GetConstructor(new Type[] { typeof(IRepository) });

                        var newBindings = new List<MemberBinding>();
                        newBindings.AddRange(bindings.Where(mb => this.Linguist.Language.IsScalar(TypeHelper.GetMemberType(mb.Member))));

                        foreach (var binding in bindings.Where(mb => !this.Linguist.Language.IsScalar(TypeHelper.GetMemberType(mb.Member))
                                                                     && ((PropertyInfo)mb.Member).GetMethod.IsVirtual
                                                                     && !((PropertyInfo)mb.Member).GetMethod.IsFinal))
                        {
                            // proxy type adds a Property of type string to hold the returned Id, prefixed with then
                            // original complex property name; e.g. Address -> AddressId
                            // need to redirect the member binding to set the new XXXId property on the proxy
                            // using the same alias, etc as the original binding
                            var previousFieldExpression = ((MemberAssignment)binding).Expression as MethodCallExpression;
                            var newPropertyName = string.Format("__{0}Id{1}", binding.Member.Name, TypeHelper.IsEnumerableOrArray(((MemberAssignment)binding).Member) ? "s" : "");
                            var newPropertyType = proxyType.GetProperty(newPropertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                            MethodInfo method = OFieldReader.GetReaderMethod(newPropertyType.PropertyType, false);
                            // pass in the actualy property name to the reader, even though the reader value will 
                            // be of a scalar string type
                            var newFieldValueExpression = Expression.Call(reader, method, Expression.Constant(mapping.GetFieldName(type, binding.Member)));
                            
                            // bind the newly emitted hidden property __XxxId to the string value on read
                            var newBinding = Expression.Bind(
                                proxyType.GetMember(newPropertyName,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0],
                                newFieldValueExpression);
                            newBindings.Add(newBinding);
                        }

                        // we need this to be a parameter and not a Constant, so that we don't end up inadvertantly caching
                        // repository instances - the parameter will be handed in by the caller when the plan is invoked
                        var repoExp = Expression.Parameter(typeof(IRepository), "repo");
                        var newExp = Expression.New(proxyCtor, repoExp);
                        init = Expression.MemberInit(newExp, newBindings.ToArray());
                        var initializer = Expression.Lambda<Func<FieldReader, IRepository, object>>(init, reader, repoExp).Compile();
                        CacheInitializer(type.EntityType, init, initializer);
                    }
                    else
                    {
                        return entry.InitializerExpression;
                    }
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
            else
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    CachedInitializer<Func<FieldReader, object>> entry;
                    if (!_nativeInitializers.TryGetValue(type.EntityType.Name, out entry))
                    {
                        // no lazy loading, so just strip out the complex type assignments from the member binder
                        var newBindings = new List<MemberBinding>();
                        newBindings.AddRange(bindings.Where(mb => this.Linguist.Language.IsScalar(TypeHelper.GetMemberType(mb.Member))));
                        init = Expression.MemberInit(Expression.New(type.EntityType), newBindings); // use default paramterless ctor
                        var initializer = Expression.Lambda<Func<FieldReader, object>>(init, reader).Compile();
                        CacheInitializer(type.EntityType, init, initializer);
                    }
                    else
                    {
                        return entry.InitializerExpression;
                    }
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }

            return init;
        }

        public IEnumerable<MemberBinding> GetDefaultBindings(IMappedEntity type)
        {
            var reader = Expression.Parameter(typeof(FieldReader), "reader");
            var mapping = OTypeMapping.FromMappedEntity(type);

            foreach (var mi in mapping.GetMappedMembers(type))
            {
                // bind each public property on the type to a call to read the reader value by name
                MethodInfo method = OFieldReader.GetReaderMethod(TypeHelper.GetMemberType(mi), false);
                yield return Expression.Bind(mi, Expression.Call(reader, method, Expression.Constant(mapping.GetFieldName(type, mi))));
            }
        }

        public MemberInitExpression GetCustomInitializer(MemberInitExpression init)
        {
            var newBindings = new List<MemberBinding>();
            newBindings.AddRange(init.Bindings.Where(mb => this.Linguist.Language.IsScalar(TypeHelper.GetMemberType(mb.Member))));
            return Expression.MemberInit(init.NewExpression, newBindings);
        }

        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        static Dictionary<string, CachedInitializer<Func<FieldReader, object>>> _nativeInitializers = new Dictionary<string, CachedInitializer<Func<FieldReader, object>>>();
        static Dictionary<string, CachedInitializer<Func<FieldReader, IRepository, object>>> _proxiedInitializers = new Dictionary<string, CachedInitializer<Func<FieldReader, IRepository, object>>>();
        public static void CacheInitializer(Type type, MemberInitExpression init, Func<FieldReader, object> initializer)
        {
            _lock.EnterWriteLock();
            try
            {
                var entry = new CachedInitializer<Func<FieldReader, object>>()
                {
                    InitializerExpression = init,
                    Initializer = initializer
                };

                if (_nativeInitializers.ContainsKey(type.Name)) // need to use mapping system here
                    _nativeInitializers[type.Name] = entry ;
                else
                    _nativeInitializers.Add(type.Name, entry);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public static void CacheInitializer(Type type, MemberInitExpression init, Func<FieldReader, IRepository, object> initializer)
        {
            _lock.EnterWriteLock();
            try
            {
                var entry = new CachedInitializer<Func<FieldReader, IRepository, object>>()
                {
                    InitializerExpression = init,
                    Initializer = initializer
                };

                if (_proxiedInitializers.ContainsKey(type.Name)) // need to use mapping system here
                    _proxiedInitializers[type.Name] = entry;
                else
                    _proxiedInitializers.Add(type.Name, entry);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public static void CacheInitializer<T>(MemberInitExpression init, Func<FieldReader, object> initializer) where T : IIdentifiable
        {
            CacheInitializer(typeof(T), init, initializer);
        }

        public static void CacheInitializer<T>(MemberInitExpression init, Func<FieldReader, IRepository, object> initializer) where T : IIdentifiable
        {
            CacheInitializer(typeof(T), init, initializer);
        }

        /// <summary>
        /// Will pre-build and pre-compile all type initializers for each type implementing IIdentifiable in the types array provided
        /// </summary>
        /// <param name="linguist"></param>
        /// <param name="types"></param>
        public static void PreBuildTypes(QueryLinguist linguist, IMapEntities mapping)
        {
            var builder = new OTypeBuilder(linguist);
            foreach(var type in mapping.Where(t => t.EntityType.Implements(typeof(IIdentifiable))))
            {
                builder.GetDefaultInitializer(type, true, mapping);
                builder.GetDefaultInitializer(type, false, mapping);
            }
        }


        class CachedInitializer<T>
        {
            public MemberInitExpression InitializerExpression;
            public T Initializer;
        }
    }
}
