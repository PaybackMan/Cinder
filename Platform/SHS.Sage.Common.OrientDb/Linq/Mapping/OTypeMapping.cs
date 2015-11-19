using SHS.Sage.Linq.Runtime;
using SHS.Sage.Mapping;
using SHS.Sage.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Mapping
{
    public class OTypeMapping : BasicMapping, IMapEntities
    {
        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        static Dictionary<Type, OTypeMapping> _repoMappings = new Dictionary<Type, OTypeMapping>();
        public static OTypeMapping Mappings<TRepository>() where TRepository : IRepository
        {
            return Mappings(typeof(Type));
        }

        public static OTypeMapping Mappings(IRepository repository)
        {
            return Mappings(repository.GetType());
        }

        private static OTypeMapping Mappings(Type type)
        {
            OTypeMapping mapping;
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_repoMappings.TryGetValue(type, out mapping))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        mapping = new OTypeMapping(type);
                        _repoMappings.Add(type, mapping);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                return mapping;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public static OTypeMapping FromMappedEntity(IMappedEntity mapped)
        {
            _lock.EnterReadLock();
            try
            {
                return _repoMappings.Values.FirstOrDefault(tm => tm.Any(me => me == mapped));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public OTypeMapping(Type repositoryType) : base(repositoryType)
        {
        }

        public IClass GetClassFromId(string id)
        {
            var builder = OSchemaBuilder.Get(this.RepositoryType);
            if (!builder.IsInitialized) throw new InvalidOperationException("The schema builder needs to be initialized.");

            var found = builder.Schema.GetClass(id);
            if (found == null) throw new InvalidOperationException("The provided Id could not be mapped to a valid class.");

            return found;
        }

        public override IMappedEntity GetEntity(Type type, Type repositoryType)
        {
            OTypeMapping mapper;
            _lock.EnterReadLock();
            try
            {
                if (_repoMappings.TryGetValue(repositoryType, out mapper))
                {
                    var entity = mapper.SingleOrDefault(e => e.EntityType.Equals(type));
                    if (entity == null)
                    {
                        // type wasn't discovered or explicitly mapped, so we'll create a default mapping for it
                        var fluentMapper = new OFluentMapper(repositoryType);
                        entity = fluentMapper.AutoMap(type);
                        mapper.AddTypeMapping(entity);
                    }
                    return entity;
                }
                else throw new InvalidOperationException("The repository type specified has not been mapped.");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IMappedEntity GetEntity(string id, Type repositoryType)
        {
            OTypeMapping mapper;
            _lock.EnterReadLock();
            try
            {
                if (_repoMappings.TryGetValue(repositoryType, out mapper))
                {
                    var oClass = GetClassFromId(id);
                    return mapper.SingleOrDefault(e => e.StorageClass.Equals(oClass.Name));
                }
                else throw new InvalidOperationException("The repository type specified has not been mapped.");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override IMappedEntity GetEntity(MemberInfo contextMember, Type repositoryType)
        {
            var type = contextMember.DeclaringType;
            if (type.Implements<IRepository>())
            {
                type = TypeHelper.GetMemberType(contextMember).GetGenericArguments()[0];
            }
            return GetEntity(type, repositoryType);
        }

        public override IEnumerable<MemberInfo> GetMappedMembers(IMappedEntity entity)
        {
            foreach(var p in entity.Properties)
            {
                yield return p.Property;
            }
        }

        public override string GetFieldName(IMappedEntity entity, MemberInfo member)
        {
            var name = entity.Properties.FirstOrDefault(p => p.Property == member || p.Property.Name.Equals(member.Name));
            if (name == null)
                return member.Name;
            return name.StorageProperty;
        }

        public override bool IsPrimaryKey(IMappedEntity entity, MemberInfo member)
        {
            return entity.EntityType.Implements(typeof(IIdentifiable)) 
                && member.Name.Equals("Id");
        }

        public override bool IsGenerated(IMappedEntity entity, MemberInfo member)
        {
            return entity.EntityType.Implements(typeof(IIdentifiable)) && member.Name.Equals("Id");
        }

        public override bool IsRelationship(IMappedEntity entity, MemberInfo member)
        {
            //if (!entity.EntityType.Implements(typeof(IIdentifiable))) return false;
            //var type = TypeHelper.GetMemberType(member);
            //return TypeImplementsTypeOrArrayOfType(type, typeof(IIdentifiable));
            return false;
        }

        public override bool IsRelationshipSource(IMappedEntity entity, MemberInfo member)
        {
            if (IsAssociationRelationship(entity, member))
            {
                if (typeof(IEnumerable).IsAssignableFrom(TypeHelper.GetMemberType(member)))
                    return false;

                // is source of relationship if relatedKeyMembers are the related entity's primary keys
                IMappedEntity entity2 = GetRelatedEntity(entity, member);
                var relatedPKs = new HashSet<string>(this.GetPrimaryKeyMembers(entity2).Select(m => m.Name));
                var relatedKeyMembers = new HashSet<string>(this.GetAssociationRelatedKeyMembers(entity, member).Select(m => m.Name));
                return relatedPKs.IsSubsetOf(relatedKeyMembers) && relatedKeyMembers.IsSubsetOf(relatedPKs);
            }
            return false;
        }

        public override bool IsRelationshipTarget(IMappedEntity entity, MemberInfo member)
        {
            if (IsAssociationRelationship(entity, member))
            {
                if (typeof(IEnumerable).IsAssignableFrom(TypeHelper.GetMemberType(member)))
                    return true;

                // is target of relationship if the assoctions keys are the same as this entities primary key
                var pks = new HashSet<string>(this.GetPrimaryKeyMembers(entity).Select(m => m.Name));
                var keys = new HashSet<string>(this.GetAssociationKeyMembers(entity, member).Select(m => m.Name));
                return keys.IsSubsetOf(pks) && pks.IsSubsetOf(keys);
            }
            return false;
        }

        private bool TypeImplementsTypeOrArrayOfType(Type sourceType, Type targetType)
        {
            if (sourceType.Implements(targetType)) return true;
            if (sourceType.IsArray)
            {
                return TypeImplementsTypeOrArrayOfType(sourceType.GetElementType(), targetType);
            }
            else if (sourceType.IsGenericType && sourceType.Implements(typeof(IEnumerable<>)))
            {
                return TypeImplementsTypeOrArrayOfType(sourceType.GetGenericArguments()[0], targetType);
            }
            else return false;
        }

        public override object GetPrimaryKey(IMappedEntity entity, object instance)
        {
            if (entity.EntityType.Implements(typeof(IIdentifiable)))
            {
                return ((IIdentifiable)instance).Id;
            }
            throw new NotSupportedException("Item must implement IIdentifiable");
        }

        public override Expression GetPrimaryKeyQuery(IMappedEntity entity, Expression source, Expression[] keys)
        {
            // make predicate
            ParameterExpression p = Expression.Parameter(entity.EntityType, "p");
            Expression pred = null;
            var idMembers = this.GetPrimaryKeyMembers(entity).ToList();
            if (idMembers.Count != keys.Length)
            {
                throw new InvalidOperationException("Incorrect number of primary key values");
            }
            for (int i = 0, n = keys.Length; i < n; i++)
            {
                MemberInfo mem = idMembers[i];
                Type memberType = TypeHelper.GetMemberType(mem);
                if (keys[i] != null && TypeHelper.GetNonNullableType(keys[i].Type) != TypeHelper.GetNonNullableType(memberType))
                {
                    throw new InvalidOperationException("Primary key value is wrong type");
                }
                Expression eq = Expression.MakeMemberAccess(p, mem).Equal(keys[i]);
                pred = (pred == null) ? eq : pred.And(eq);
            }
            var predLambda = Expression.Lambda(pred, p);

            return Expression.Call(typeof(Queryable), "SingleOrDefault", new Type[] { entity.EntityType }, source, predLambda);
        }

        public override IEnumerable<IdentifiableInfo> GetDependentEntities(IMappedEntity entity, object instance)
        {
            foreach (var mi in this.GetMappedMembers(entity))
            {
                if (this.IsRelationship(entity, mi) && this.IsRelationshipSource(entity, mi))
                {
                    IMappedEntity relatedEntity = this.GetRelatedEntity(entity, mi);
                    var value = mi.GetValue(instance);
                    if (value != null)
                    {
                        var list = value as IList;
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (item != null)
                                {
                                    yield return new IdentifiableInfo(item, relatedEntity);
                                }
                            }
                        }
                        else
                        {
                            yield return new IdentifiableInfo(value, relatedEntity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The type of the entity on the other side of the relationship
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public override IMappedEntity GetRelatedEntity(IMappedEntity entity, MemberInfo member)
        {
            Type relatedType = TypeHelper.GetElementType(TypeHelper.GetMemberType(member));
            return this.GetEntity(relatedType, RepositoryType);
        }


        /// <summary>
        /// Deterimines is a property is mapped onto a column or relationship
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public override bool IsMapped(IMappedEntity entity, MemberInfo member)
        {
            return true;
        }

        /// <summary>
        /// Determines if a property is mapped onto a column
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsColumn(IMappedEntity entity, MemberInfo member)
        {
            return this.IsMapped(entity, member) && this.IsScalar(TypeHelper.GetMemberType(member));
        }

        public override bool IsAssociationRelationship(IMappedEntity entity, MemberInfo member)
        {
            if (IsMapped(entity, member) && !IsColumn(entity, member))
            {
                Type otherType = TypeHelper.GetElementType(TypeHelper.GetMemberType(member));
                return !this.IsScalar(otherType);
            }
            return false;
        }

        private bool IsScalar(Type type)
        {
            type = TypeHelper.GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return false;
                case TypeCode.Object:
                    return
                        type == typeof(DateTimeOffset) ||
                        type == typeof(TimeSpan) ||
                        type == typeof(Guid) ||
                        type == typeof(byte[]) ||
                        type.Implements(typeof(IIdentifiable));
                default:
                    return true;
            }
        }

        public override IEnumerable<MemberInfo> GetAssociationRelatedKeyMembers(IMappedEntity entity, MemberInfo member)
        {
            List<MemberInfo> keyMembers;
            List<MemberInfo> relatedKeyMembers;
            this.GetAssociationKeys(entity, member, out keyMembers, out relatedKeyMembers);
            return relatedKeyMembers;
        }

        private void GetAssociationKeys(IMappedEntity entity, MemberInfo member, out List<MemberInfo> keyMembers, out List<MemberInfo> relatedKeyMembers)
        {
            // member = Property/Field on type that maps to related instance.  e.g.  Patient.Address => member is .Address
            // keyMembers = for relational schemas, these would be primary key fields on source type, for graph systems, this is just the member
            // relatedKeyMembers = for relational schemas, these would be the foreign key fields on target type, this is Id on the target type
            IMappedEntity entity2 = GetRelatedEntity(entity, member);

            // find all members in common (same name)
            //var map1 = this.GetMappedMembers(entity).Where(m => this.IsColumn(entity, m)).ToDictionary(m => m.Name);
            //var map2 = this.GetMappedMembers(entity2).Where(m => this.IsColumn(entity2, m)).ToDictionary(m => m.Name);
            //var commonNames = map1.Keys.Intersect(map2.Keys).OrderBy(k => k);
            keyMembers = new List<MemberInfo>();
            relatedKeyMembers = new List<MemberInfo>();
            //foreach (string name in commonNames)
            //{
            //    keyMembers.Add(map1[name]);
            //    relatedKeyMembers.Add(map2[name]);
            //}
            keyMembers.Add(member);
            relatedKeyMembers.AddRange(this.GetMappedMembers(entity2).Where(m => this.IsPrimaryKey(entity2, m)));
        }

        public override IEnumerable<MemberInfo> GetAssociationKeyMembers(IMappedEntity entity, MemberInfo member)
        {
            List<MemberInfo> keyMembers;
            List<MemberInfo> relatedKeyMembers;
            this.GetAssociationKeys(entity, member, out keyMembers, out relatedKeyMembers);
            return keyMembers;
        }

        public override IEnumerable<IdentifiableInfo> GetDependingEntities(IMappedEntity entity, object instance)
        {
            foreach (var mi in this.GetMappedMembers(entity))
            {
                if (this.IsRelationship(entity, mi) && this.IsRelationshipTarget(entity, mi))
                {
                    IMappedEntity relatedEntity = this.GetRelatedEntity(entity, mi);
                    var value = mi.GetValue(instance);
                    if (value != null)
                    {
                        var list = value as IList;
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (item != null)
                                {
                                    yield return new IdentifiableInfo(item, relatedEntity);
                                }
                            }
                        }
                        else
                        {
                            yield return new IdentifiableInfo(value, relatedEntity);
                        }
                    }
                }
            }
        }

        public override bool IsModified(IMappedEntity entity, object instance, object original)
        {
            foreach (var mi in this.GetMappedMembers(entity))
            {
                if (this.IsColumn(entity, mi))
                {
                    if (!object.Equals(mi.GetValue(instance), mi.GetValue(original)))
                        return true;
                }
            }
            return false;
        }

        public override QueryMapper CreateMapper(QueryTranslator translator)
        {
            return new OTypeMapper(this, translator);
        }


        List<IMappedEntity> _mappings = new List<IMappedEntity>();

        public void AddTypeMapping(IMappedEntity mapping)
        {
            _mappings.Add(mapping);
        }

        public void RemoveTypeMapping(IMappedEntity mapping)
        {
            _mappings.Remove(mapping);
        }

        public IEnumerator<IMappedEntity> GetEnumerator()
        {
            return _mappings.AsReadOnly().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsInitialized { get; set; }
    }
}
