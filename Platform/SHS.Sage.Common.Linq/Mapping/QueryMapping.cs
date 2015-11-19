using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Mapping
{
    /// <summary>
    /// Defines mapping information & rules for the query provider
    /// </summary>
    public abstract class QueryMapping
    {
        public Type RepositoryType { get; protected set; }
        /// <summary>
        /// Determines the storage class name (i.e. vertex or table name) based on the type of the entity alone
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual string GetStorageClass(Type type)
        {
            return type.Name;
        }

        /// <summary>
        /// Get the meta entity directly corresponding to the CLR type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract IMappedEntity GetEntity(Type type, Type repositoryType);
        //{
        //    return this.GetEntity(type, this.GetStorageClass(type));
        //}

        /// <summary>
        /// Get the meta entity that maps between the CLR type 'entityType' and the storage class (i.e. table, vertex, etc), yet
        /// is represented publicly as 'elementType'.
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="storageClass"></param>
        /// <returns></returns>
        //public abstract MappingEntity GetEntity(Type elementType, string storageClass);

        /// <summary>
        /// Get the meta entity represented by the IQueryable context member
        /// </summary>
        /// <param name="contextMember"></param>
        /// <returns></returns>
        public abstract IMappedEntity GetEntity(MemberInfo contextMember, Type repositoryType);

        public abstract IEnumerable<MemberInfo> GetMappedMembers(IMappedEntity entity);

        public abstract bool IsPrimaryKey(IMappedEntity entity, MemberInfo member);

        public virtual IEnumerable<MemberInfo> GetPrimaryKeyMembers(IMappedEntity entity)
        {
            return this.GetMappedMembers(entity).Where(m => this.IsPrimaryKey(entity, m));
        }

        /// <summary>
        /// Determines if a property is mapped as a relationship
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public abstract bool IsRelationship(IMappedEntity entity, MemberInfo member);

        /// <summary>
        /// Determines if a relationship property refers to a single entity (as opposed to a collection.)
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsSingletonRelationship(IMappedEntity entity, MemberInfo member)
        {
            if (!this.IsRelationship(entity, member))
                return false;
            Type ieType = TypeHelper.FindIEnumerable(TypeHelper.GetMemberType(member));
            return ieType == null;
        }

        /// <summary>
        /// Determines whether a given expression can be executed locally. 
        /// (It contains no parts that should be translated to the target environment.)
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual bool CanEvaluateLocally(Expression expression)
        {
            // any operation on a query can't be done locally
            ConstantExpression cex = expression as ConstantExpression;
            if (cex != null)
            {
                IQueryable query = cex.Value as IQueryable;
                if (query != null && query.Provider.GetType().IsSubclassOf(typeof(QueryProvider)))
                    return false;
            }
            MethodCallExpression mc = expression as MethodCallExpression;
            if (mc != null &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable) ||
                 mc.Method.DeclaringType.Implements(typeof(IRepository))
                 ))
            {
                return false;
            }
            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
                return true;
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

        public abstract object GetPrimaryKey(IMappedEntity entity, object instance);
        public abstract Expression GetPrimaryKeyQuery(IMappedEntity entity, Expression source, Expression[] keys);
        public abstract IEnumerable<IdentifiableInfo> GetDependentEntities(IMappedEntity entity, object instance);
        public abstract IEnumerable<IdentifiableInfo> GetDependingEntities(IMappedEntity entity, object instance);
        public abstract bool IsModified(IMappedEntity entity, object instance, object original);

        public abstract QueryMapper CreateMapper(QueryTranslator translator);
    }
}
