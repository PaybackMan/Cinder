using SHS.Sage.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Mapping
{
    public abstract class BasicMapping : QueryMapping
    {
        protected BasicMapping(Type repositoryType)
        {
            this.RepositoryType = repositoryType;
        }

        public override IMappedEntity GetEntity(Type type, Type repositoryType)
        {
            return new BasicMappedEntity(type, type.Name, repositoryType);
        }

        public override IMappedEntity GetEntity(MemberInfo contextMember, Type repositoryType)
        {
            Type elementType = TypeHelper.GetElementType(TypeHelper.GetMemberType(contextMember));
            return this.GetEntity(elementType, repositoryType);
        }

        public override bool IsRelationship(IMappedEntity entity, MemberInfo member)
        {
            return this.IsAssociationRelationship(entity, member);
        }

        /// <summary>
        /// Deterimines is a property is mapped onto a Field or relationship
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsMapped(IMappedEntity entity, MemberInfo member)
        {
            return true;
        }

        /// <summary>
        /// Determines if a property is mapped onto a Field
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsField(IMappedEntity entity, MemberInfo member)
        {
            //return this.mapping.IsMapped(entity, member) && this.translator.Linguist.Language.IsScalar(TypeHelper.GetMemberType(member));
            return this.IsMapped(entity, member);
        }

        /// <summary>
        /// The type declaration for the Field in the provider's syntax
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns>a string representing the type declaration or null</returns>
        public virtual string GetFieldDbType(IMappedEntity entity, MemberInfo member)
        {
            return null;
        }

        /// <summary>
        /// Determines if a property represents or is part of the entities unique identity (often primary key)
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public override bool IsPrimaryKey(IMappedEntity entity, MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// Determines if a property is computed after insert or update
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsComputed(IMappedEntity entity, MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// Determines if a property is generated on the server during insert
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsGenerated(IMappedEntity entity, MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// Determines if a property can be part of an update operation
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsUpdatable(IMappedEntity entity, MemberInfo member)
        {
            return !this.IsPrimaryKey(entity, member);
        }

        /// <summary>
        /// The type of the entity on the other side of the relationship
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual IMappedEntity GetRelatedEntity(IMappedEntity entity, MemberInfo member)
        {
            Type relatedType = TypeHelper.GetElementType(TypeHelper.GetMemberType(member));
            return this.GetEntity(relatedType, entity.RepositoryType);
        }

        /// <summary>
        /// Determines if the property is an assocation relationship.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsAssociationRelationship(IMappedEntity entity, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Field)
                return IsAssociationType(((FieldInfo)member).FieldType);
            else return IsAssociationType(((PropertyInfo)member).PropertyType);
        }

        protected virtual bool IsAssociationType(Type type)
        {
            if (type.Implements(typeof(IEnumerable<>)))
            {
                return IsAssociationType(type.GetTypeInfo().ImplementedInterfaces
                    .Single(i => i.IsGenericType && i.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
                    .GetGenericArguments()[0]);
            }
            else return type.Implements(typeof(IIdentifiable));
        }

        /// <summary>
        /// Returns the key members on this side of the association
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual IEnumerable<MemberInfo> GetAssociationKeyMembers(IMappedEntity entity, MemberInfo member)
        {
            return new MemberInfo[] { };
        }

        /// <summary>
        /// Returns the key members on the other side (related side) of the association
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual IEnumerable<MemberInfo> GetAssociationRelatedKeyMembers(IMappedEntity entity, MemberInfo member)
        {
            return new MemberInfo[] { };
        }

        public abstract bool IsRelationshipSource(IMappedEntity entity, MemberInfo member);

        public abstract bool IsRelationshipTarget(IMappedEntity entity, MemberInfo member);

        /// <summary>
        /// The name of the corresponding database table
        /// </summary>
        /// <param name="rowType"></param>
        /// <returns></returns>
        public virtual string GetTableName(IMappedEntity entity)
        {
            return entity.StorageClass;
        }

        /// <summary>
        /// The name of the corresponding table Field
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual string GetFieldName(IMappedEntity entity, MemberInfo member)
        {
            return member.Name;
        }

        /// <summary>
        /// A sequence of all the mapped members
        /// </summary>
        /// <param name="rowType"></param>
        /// <returns></returns>
        public override IEnumerable<MemberInfo> GetMappedMembers(IMappedEntity entity)
        {
            //Type type = entity.ElementType.IsInterface ? entity.EntityType : entity.ElementType;
            Type type = entity.EntityType;
            HashSet<MemberInfo> members = new HashSet<MemberInfo>(type.GetFields().Cast<MemberInfo>().Where(m => this.IsMapped(entity, m)));
            members.UnionWith(type.GetProperties().Cast<MemberInfo>().Where(m => this.IsMapped(entity, m)));
            return members.OrderBy(m => m.Name);
        }

        public override bool IsModified(IMappedEntity entity, object instance, object original)
        {
            foreach (var mi in this.GetMappedMembers(entity))
            {
                if (this.IsField(entity, mi))
                {
                    if (!object.Equals(mi.GetValue(instance), mi.GetValue(original)))
                        return true;
                }
            }
            return false;
        }

        public override object GetPrimaryKey(IMappedEntity entity, object instance)
        {
            object firstKey = null;
            List<object> keys = null;
            foreach (var mi in this.GetPrimaryKeyMembers(entity))
            {
                if (firstKey == null)
                {
                    firstKey = mi.GetValue(instance);
                }
                else
                {
                    if (keys == null)
                    {
                        keys = new List<object>();
                        keys.Add(firstKey);
                    }
                    keys.Add(mi.GetValue(instance));
                }
            }
            if (keys != null)
            {
                return new CompoundKey(keys.ToArray());
            }
            return firstKey;
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

        public override QueryMapper CreateMapper(QueryTranslator translator)
        {
            return new BasicMapper(this, translator);
        }
    }

    public class BasicMappedEntity : MappingEntity
    {
        string entityID;
        Type type;
        ReadOnlyCollection<IMappedProperty> properties;
        Type repoType;

        public BasicMappedEntity(Type type, string storageClass, Type repositoryType, params IMappedProperty[] properties)
        {
            this.entityID = storageClass;
            this.type = type;
            this.properties = properties.ToReadOnly();
            this.repoType = repositoryType;
        }

        public override string StorageClass
        {
            get { return this.entityID; }
            set { this.entityID = value; }
        }

        public override Type EntityType
        {
            get { return this.type; }
        }

        public override IEnumerable<IMappedProperty> Properties
        {
            get
            {
                return this.properties;
            }
        }

        public override Type RepositoryType
        {
            get
            {
                return this.repoType;
            }
        }
    }
}
