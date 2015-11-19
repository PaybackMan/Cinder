using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHS.Sage.Mapping;
using System.Threading;
using SHS.Sage.Linq.Mapping;
using SHS.Sage.Linq.Language;
using System.Diagnostics;

namespace SHS.Sage.Schema
{
    /// <summary>
    /// Numeric data type values returned from OrientDb
    /// </summary>
    public enum OPropertyType : int
    {
        Boolean = 0,
        Integer = 1,
        Short = 2,
        Long = 3,
        Float = 4,
        Double = 5,
        DateTime = 6,
        String = 7,
        Binary = 8,
        Embedded = 9,
        EmbeddedList = 10,
        EmbeddedSet = 11,
        EmbeddedMap = 12,
        Link = 13,
        LinkList = 14,
        LinkSet = 15,
        LinkMap = 16,
        Date = 19,
        Decimal = 21
    }

    public abstract class OSchemaBuilder : IBuildSchema
    {
        public void Build(IRepository repository, Type identifiableType)
        {
            throw new NotImplementedException();
        }
        public void Build<T>(IRepository repository) where T : IIdentifiable
        {
            throw new NotImplementedException();
        }
        public abstract void BuildSchema(IRepository repository);
        public abstract ISchema<IRepository> GetSchema(IRepository repository);

        public bool IsBuilt(IRepository repository, Type identifiableType)
        {
            throw new NotImplementedException();
        }

        public bool IsBuilt<T>(IRepository repository) where T : IIdentifiable
        {
            throw new NotImplementedException();
        }


        public ISchema Schema { get; protected set; }
        public bool IsInitialized { get; protected set; }

        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        static Dictionary<Type, IBuildSchema> _builders = new Dictionary<Type, IBuildSchema>();
        public static IBuildSchema Get(Type repositoryType)
        {
            IBuildSchema builder;
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_builders.TryGetValue(repositoryType, out builder))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        builder = (IBuildSchema)Activator.CreateInstance(typeof(OSchemaBuilder<>).MakeGenericType(repositoryType));
                        _builders.Add(repositoryType, builder);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                return builder;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
    }

    public class OSchemaBuilder<TRepository> : OSchemaBuilder, IBuildSchema<TRepository>
        where TRepository : ORepository
    {
        
        public void Build(TRepository repository, Type identifiableType)
        {
            var mappingSystem = OTypeMapping.Mappings(repository);
            var mapping = mappingSystem.GetEntity(identifiableType, repository.GetType());
            Build(repository, mapping, mappingSystem);
        }

        public void Build<T>(TRepository repository) where T : IIdentifiable
        {
            Build(repository, typeof(T));
        }

        public void BuildSchema(TRepository repository)
        {
            var schema = GetSchema(repository);
        }

        public override void BuildSchema(IRepository repository)
        {
            BuildSchema((TRepository)repository);
        }

        public override ISchema<IRepository> GetSchema(IRepository repository)
        {
            return (ISchema<IRepository>)GetSchema((TRepository)repository);
        }

        public ISchema<TRepository> GetSchema(TRepository repository)
        {
            reload:
            var query = "select expand(classes) from metadata:schema";
            var schema = new OSchema<TRepository>();

            using (var reader = repository.ExecuteReader(query))
            {
                while(reader.Read())
                {
                    var superClass = reader.GetString(reader.GetOrdinal("superClass"));
                    OClass<TRepository> oClass = null;

                    if (superClass == "V")
                    {
                        oClass = new OClass<TRepository>(OClass<TRepository>.V)
                        {
                            ClassType = ClassType.Thing
                        };
                    }
                    else if (superClass == "E")
                    {
                        oClass = new OClass<TRepository>(OClass<TRepository>.E)
                        {
                            ClassType = ClassType.Association
                        };
                    }
                    else if (!string.IsNullOrEmpty(superClass))
                    {
                        oClass = new OClass<TRepository>(superClass);
                    }

                    if (oClass != null)
                    {
                        oClass.Name = reader.GetString(reader.GetOrdinal("name"));
                        oClass.Clusters = ((IList<object>)reader.GetValue(reader.GetOrdinal("clusterIds"))).Select(c => c.ToString()).ToArray();
                        oClass.DefaultCluster = reader.GetInt32(reader.GetOrdinal("defaultClusterId")).ToString();
                        oClass.IsAbstract = reader.GetBoolean(reader.GetOrdinal("abstract"));
                        foreach (var propDef in (IList<object>)reader.GetValue(reader.GetOrdinal("properties")))
                        {
                            if (propDef == null) break;
                            var prop = new OProperty();
                            foreach(var kvp in (IEnumerable<KeyValuePair<string, object>>)propDef)
                            {
                                switch(kvp.Key)
                                {
                                    case "name":
                                        {
                                            prop.Name = kvp.Value.ToString();
                                            break;
                                        }
                                    case "notNull":
                                        {
                                            prop.IsNotNull = (bool)kvp.Value;
                                            break;
                                        }
                                    case "readonly":
                                        {
                                            prop.IsReadOnly = (bool)kvp.Value;
                                            break;
                                        }
                                    case "mandatory":
                                        {
                                            prop.IsRequired = (bool)kvp.Value;
                                            break;
                                        }
                                    case "type":
                                        {
                                            prop.PropertyType = (int)kvp.Value;
                                            break;
                                        }
                                }
                            }
                            oClass.AddProperty(prop);
                        }
                        schema.AddClass(oClass);
                    }
                }
            }

            var unknowns = schema.Classes.Where(c => c.ClassType == ClassType.Unknown).ToArray();
            foreach(var oClass in unknowns)
            {
                AssignClassType((OClass<TRepository>)oClass, schema);
            }

            base.Schema = schema;

            if (UpdateSchema(repository))
            {
                goto reload; // schema changed, so reload everything
            }

            base.IsInitialized = true;

            return schema;
        }

        HashSet<Type> _visited = new HashSet<Type>();
        protected virtual bool UpdateSchema(TRepository repository)
        {
        retry:
            try
            {
                var hasChanges = false;
                var mappingSystem = OTypeMapping.Mappings(repository);
                foreach (var mapping in mappingSystem)
                {
                    hasChanges |= UpdateMapping(repository, mappingSystem, mapping);
                }
                return hasChanges;
            }
            catch (InvalidOperationException ioe)
            {
                if (ioe.Message.Contains("Collection was modified"))
                {
                    // this can happen if we discover unmapped types during the property traversal
                    // so just go back and try again - it will skip types already processed
                    goto retry;
                }
                else throw;
            }
        }

        protected virtual bool UpdateMapping(TRepository repository, OTypeMapping mappingSystem, IMappedEntity mapping)
        {
            if (_visited.Contains(mapping.EntityType)) return false;

            if (mapping.EntityType.BaseType == null
                || mapping.EntityType.BaseType == typeof(object)
                || _visited.Contains(mapping.EntityType.BaseType))
            {
                // type's base type is Object or Base Type is already defined in schema,
                // so don't recurse any more
                _visited.Add(mapping.EntityType);
                return Build(repository, mapping, mappingSystem);
            }
            else
            {
                // recurse down a level
                var baseEntity = mappingSystem.GetEntity(mapping.EntityType.BaseType, repository.GetType());
                return UpdateMapping(repository, mappingSystem, baseEntity);
            }
        }

        protected virtual bool Build(TRepository repository, IMappedEntity mapping, OTypeMapping mappingSystem)
        {
            try
            {
                var current = Schema.Classes.FirstOrDefault(c => c.Name.Equals(mapping.StorageClass));
                Debug.WriteLine("--- Building " + mapping.StorageClass + " Schema");
                if (current == null)
                {
                    // doesn't exist - create new class
                    CreateClass(repository, mapping, mappingSystem);
                    return true;
                }
                else
                {
                    return UpdateClass(repository, mapping, mappingSystem, current);
                }
            }
            finally
            {
                Debug.WriteLine("--- Built " + mapping.StorageClass + " Schema");
            }
        }

        protected virtual bool UpdateClass(TRepository repository, IMappedEntity mapping, OTypeMapping mappingSystem, IClass<TRepository> current)
        {
            var deletes = current.Properties.Where(p => !mapping.Properties.Any(mp => mp.StorageProperty.Equals(p.Name)));
            var adds = mapping.Properties.Where(mp => !current.Properties.Any(p => mp.StorageProperty.Equals(p.Name)));
            var result = adds.Count() > 0 || deletes.Count() > 0;
            foreach(var delete in deletes)
            {
                DropProperty(repository, current, delete, mappingSystem);
            }
            foreach(var add in adds)
            {
                CreateProperty(repository, mapping, add, mappingSystem);
            }
            return result;
        }

        protected virtual void CreateClass(TRepository repository, IMappedEntity mapping, OTypeMapping mappingSystem)
        {
            if (mapping.EntityType == typeof(IIdentifiable)
                || mapping.StorageClass == "E"
                || mapping.StorageClass == "V")
                return;

            if (mapping.EntityType.Implements<IAssociation>())
            {
                CreateEdge(repository, mapping, mappingSystem);
            }
            else if (mapping.EntityType.Implements<IIdentifiable>())
            {
                CreateVertex(repository, mapping, mappingSystem);
            }
        }

        protected virtual void CreateEdge(TRepository repository, IMappedEntity mapping, OTypeMapping mappingSystem)
        {
            var createClassCommand = "CREATE CLASS {0} EXTENDS {1};";
            var baseMapping = mapping.EntityType.BaseType == typeof(object)
                ? null
                : mappingSystem.GetEntity(mapping.EntityType.BaseType, repository.GetType());
            // create the vertex table
            repository.ExecuteStatement(string.Format(createClassCommand, mapping.StorageClass, baseMapping == null ? "E" : baseMapping.StorageClass));
            foreach (var mappedProperty in mapping.Properties)
            {
                CreateProperty(repository, mapping, mappedProperty, mappingSystem);
            }
        }

        protected virtual void CreateVertex(TRepository repository, IMappedEntity mapping, OTypeMapping mappingSystem)
        {
            if (mapping.EntityType.BaseType == null)
                return; // we don't map interface types directly

            var createClassCommand = "CREATE CLASS {0} EXTENDS {1};";

            var baseMapping = mapping.EntityType.BaseType == typeof(object)
                ? null
                : mappingSystem.GetEntity(mapping.EntityType.BaseType, repository.GetType());
            // create the vertex table
            repository.ExecuteStatement(string.Format(createClassCommand, mapping.StorageClass, baseMapping == null ? "V" : baseMapping.StorageClass));
            foreach(var mappedProperty in mapping.Properties.Where(pi => pi.Property.DeclaringType == mapping.EntityType))
            {
                if (mappedProperty.StorageProperty == "Id") continue;
                CreateProperty(repository, mapping, mappedProperty, mappingSystem);
            }
        }

        protected virtual void CreateProperty(TRepository repository, IMappedEntity mapping, IMappedProperty mappedProperty, OTypeMapping mappingSystem)
        {
            // CREATE PROPERTY <class>.<property> <type> [<linked-type>|<linked-class>] [UNSAFE]
            var createPropertyCommand = "CREATE PROPERTY {0}.{1} {2};";
            var storageType = new OStorageType(mappedProperty.Property.PropertyType);
            var storageTypeString = "";
            switch(storageType.ToInt32())
            {
                default:
                    {
                        storageTypeString = storageType.TypeName.ToUpper();
                        break;
                    }
                case (int)ODataType.LinkSet:
                    {
                        var dependentType = TypeSystem.GetElementType(mappedProperty.Property.PropertyType);
                        var dependentMapping = mappingSystem.GetEntity(dependentType, repository.GetType());
                        // make sure this guy has been built alread
                        UpdateMapping(repository, mappingSystem, dependentMapping);
                        storageTypeString = storageType.TypeName.ToUpper() + " " + dependentMapping.StorageClass;
                        break;
                    }
                case (int)ODataType.Link:
                    {
                        var dependentMapping = mappingSystem.GetEntity(mappedProperty.Property.PropertyType, repository.GetType());
                        // make sure this guy has been built alread
                        UpdateMapping(repository, mappingSystem, dependentMapping);
                        storageTypeString = storageType.TypeName.ToUpper() + " " + dependentMapping.StorageClass;
                        break;
                    }
                case (int)ODataType.Unknown:
                    {
                        throw new NotSupportedException("Property " + mappedProperty.Property.Name + " of type " + mappedProperty.Property.PropertyType.ToString() + " is not supported.");
                    }
            }
            repository.ExecuteStatement(string.Format(createPropertyCommand, mapping.StorageClass, mappedProperty.StorageProperty, storageTypeString));
        }

        protected virtual void DropProperty(TRepository repository, IClass storageClass, IProperty storageProperty, OTypeMapping mappingSystem)
        {
            var dropPropertyCommand = "CREATE PROPERTY {0}.{1}";
            repository.ExecuteStatement(string.Format(dropPropertyCommand, storageClass.Name, storageProperty.Name));
        }

        private ClassType AssignClassType(OClass<TRepository> oClass, OSchema<TRepository> schema)
        {
            if (oClass.ClassType == ClassType.Unknown)
            {
                var baseClass = schema.Classes.SingleOrDefault(bc => bc.Name == oClass.BaseClass);
                if (baseClass == null) return ClassType.Unknown;

                var classType = AssignClassType((OClass<TRepository>)baseClass, schema);
                oClass.ClassType = classType;
                return classType;
            }
            else
            {
                return oClass.ClassType;
            }
        }

        public new ISchema<TRepository> Schema { get { return base.Schema as ISchema<TRepository>; } }
    }
}
