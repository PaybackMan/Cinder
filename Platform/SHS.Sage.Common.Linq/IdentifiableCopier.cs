using SHS.Sage.Linq;
using SHS.Sage.Linq.Runtime;
using SHS.Sage.UnitOfWork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public class IdentifiableCopier : ICopyIdentifiables
    {
        static Dictionary<Type, IdentifiableFunctions> _funcs = new Dictionary<Type, IdentifiableFunctions>();
        static ReaderWriterLockSlim _rwls = new ReaderWriterLockSlim();

        public IRepository Repository { get; private set; }

        public IdentifiableCopier(IRepository repository)
        {
            this.Repository = repository;
        }

        public virtual IIdentifiable Clone(IIdentifiable source)
        {
            var type = source.GetType();
            if (source is ICloneable)
            {
                return (IIdentifiable)((ICloneable)source).Clone();
            }
            else
            {
                return GetCloner(type, Repository)(this.Repository, source);
            }
        }

        public virtual void Copy(IIdentifiable source, IIdentifiable destination)
        {
            var type = source.GetType();
            var destType = destination.GetType();
            if (type != destType && type.IsSubclassOf(destType))
            {
                type = destType;
            }
            if (source is ICopyable)
            {
                ((ICopyable)destination).CopyFrom(source);
            }
            else
            {
                GetCopier(type)(source, destination);
            }
        }

        public virtual IIdentifiable Copy(IIdentifiable source)
        {
            var copy = Clone(source);
            Copy(source, copy);
            return copy;
        }

        /// <summary>
        /// Gets/creates a compiled expression to copy all Field/Property values from one instance to another instance of the same type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Action<IIdentifiable, IIdentifiable> GetCopier(Type type)
        {
            _rwls.EnterUpgradeableReadLock();
            try
            {
                IdentifiableFunctions funcs;
                Action<IIdentifiable, IIdentifiable> copier;


                if (!_funcs.TryGetValue(type, out funcs))
                {
                    funcs = new IdentifiableFunctions();
                    _funcs.Add(type, funcs);
                }

                if (funcs.Copy != null)
                {
                    return funcs.Copy;
                }

                _rwls.EnterWriteLock();
                try
                {
                    bool isProxy = type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IProxyIdentifiable));
                    var identifiableParam = Expression.Parameter(typeof(IIdentifiable), "source");
                    var copyToParam = Expression.Parameter(typeof(IIdentifiable), "destination");
                    var propertyAssignments = new List<Expression>();
                    var actualParam = Expression.Convert(identifiableParam, type);
                    var reflectingType = type;

                    while (reflectingType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable)))
                    {
                        foreach (var mi in reflectingType.GetTypeInfo().DeclaredMembers)
                        {
                            if (mi.GetCustomAttribute<IgnoreAttribute>() != null) continue; // ignore ignored members

                            if (mi is PropertyInfo && ((PropertyInfo)mi).CanWrite && ((PropertyInfo)mi).CanRead)
                            {
                                var sourceField = Expression.Property(Expression.Convert(identifiableParam, type), (PropertyInfo)mi);
                                var destField = Expression.Property(Expression.Convert(copyToParam, type), (PropertyInfo)mi);
                                propertyAssignments.Add(Expression.Assign(destField, sourceField));
                            }
                            else if (mi is FieldInfo)
                            {
                                if (mi.Name.Contains("<")) continue; // this is a compiler emitted backing field, ignore it
                                var sourceField = Expression.Field(Expression.Convert(identifiableParam, type), (FieldInfo)mi);
                                var destField = Expression.Field(Expression.Convert(copyToParam, type), (FieldInfo)mi);
                                propertyAssignments.Add(Expression.Assign(destField, sourceField));
                            }
                        }
                        reflectingType = reflectingType.GetTypeInfo().BaseType; // go down a level
                    }

                    var assignBlock = Expression.Block(propertyAssignments);

                    var lambda = Expression.Lambda<Action<IIdentifiable, IIdentifiable>>(assignBlock, identifiableParam, copyToParam);
                    copier = lambda.Compile();
                    funcs.Copy = copier;
                    return copier;
                }
                finally
                {
                    _rwls.ExitWriteLock();
                }
            }
            finally
            {
                _rwls.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Gets/creates a compiled expression used to create a new instance of a type from an existing instance of the same type, copying only public property values
        /// </summary>
        /// <param name="type"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        private static Func<IRepository, IIdentifiable, IIdentifiable> GetCloner(Type type, IRepository repository)
        {
            _rwls.EnterUpgradeableReadLock();
            try
            {
                IdentifiableFunctions funcs;
                Func<IRepository, IIdentifiable, IIdentifiable> cloner;
                bool isProxy = type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IProxyIdentifiable));

                if (!_funcs.TryGetValue(type, out funcs))
                {
                    funcs = new IdentifiableFunctions();
                    _funcs.Add(type, funcs);
                }

                if (funcs.Clone != null)
                {
                    return funcs.Clone;
                }

                _rwls.EnterWriteLock();
                try
                {
                    var repoParam = Expression.Parameter(typeof(IRepository));
                    var identifiableParam = Expression.Parameter(typeof(IIdentifiable));
                    var propertyAssignments = new List<MemberBinding>();
                    var newType = isProxy ? Expression.New(type.GetTypeInfo().DeclaredConstructors.First(), repoParam) : Expression.New(type);
                    var actualParam = Expression.Convert(identifiableParam, type);
                    var reflectingType = type;

                    while (reflectingType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable)))
                    {
                        foreach (var pi in reflectingType.GetTypeInfo().DeclaredProperties.Where(p => p.CanRead && p.CanWrite))
                        {
                            if (pi.GetCustomAttribute<IgnoreAttribute>() != null) continue; // ignore ignored members

                            MemberBinding binding = null;
                            var piTypeInfo = pi.PropertyType.GetTypeInfo();
                            if (pi.PropertyType.IsArray
                                && piTypeInfo.GetElementType().GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable)))
                            {
                                binding = GetArrayBinding(actualParam, pi, repoParam);
                            }
                            else if (piTypeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable))
                                && piTypeInfo.IsGenericType
                                && (piTypeInfo.GenericTypeArguments[0] == typeof(IIdentifiable)
                                    || piTypeInfo.GenericTypeArguments[0].GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable))))
                            {
                                binding = GetEnumerableBinding(actualParam, pi, repoParam);
                            }
                            else if (!piTypeInfo.IsInterface && piTypeInfo.ImplementedInterfaces.Contains(typeof(IIdentifiable)))
                            {
                                binding = GetIdentifiableBinding(actualParam, pi, repoParam);
                            }
                            else
                            {
                                binding = Expression.Bind(pi, Expression.MakeMemberAccess(actualParam, pi));
                            }
                            propertyAssignments.Add(binding);
                        }
                        reflectingType = reflectingType.GetTypeInfo().BaseType; // go down a level
                    }

                    var newInit = Expression.MemberInit(newType, propertyAssignments);
                    var lambda = Expression.Lambda<Func<IRepository, IIdentifiable, IIdentifiable>>(newInit, repoParam, identifiableParam);
                    cloner = lambda.Compile();
                    funcs.Clone = cloner;
                    return cloner;
                }
                finally
                {
                    _rwls.ExitWriteLock();
                }
            }
            finally
            {
                _rwls.ExitUpgradeableReadLock();
            }
        }

        private static MemberBinding GetIdentifiableBinding(UnaryExpression identifiableParam, PropertyInfo pi, ParameterExpression repoParam)
        {
            var type = pi.PropertyType;
            var idProperty = type.GetTypeInfo().GetPublicProperty("Id");
            var identifiableProperty = Expression.MakeMemberAccess(identifiableParam, pi);
            var idPropertyAccessor = Expression.MakeMemberAccess(identifiableProperty, idProperty);
            var idBinding = Expression.Bind(idProperty, idPropertyAccessor);

            MemberInitExpression newInit;
            if (type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IProxyIdentifiable)))
            {
                var ctorExp = Expression.New(type.GetTypeInfo().DeclaredConstructors.Single(c => c.GetParameters()[0].ParameterType.Equals(typeof(IRepository))), repoParam);
                newInit = Expression.MemberInit(ctorExp, idBinding);
            }
            else
            {
                newInit = Expression.MemberInit(Expression.New(type), idBinding);
            }

            var instanceIdSafe = Expression.Condition(Expression.Equal(identifiableProperty, Expression.Convert(Expression.Constant(null), type)),
                Expression.Convert(Expression.Constant(null), typeof(string)),
                idPropertyAccessor);

            var idOrInstanceEqualsNull = Expression.Or(
                Expression.Equal(identifiableProperty, Expression.Constant(null)),
                Expression.Equal(instanceIdSafe, Expression.Constant(null)));

            var ifNullThenDefault = Expression.Condition(idOrInstanceEqualsNull,
                Expression.Convert(Expression.Constant(null), type),
                newInit);

            return Expression.Bind(pi, ifNullThenDefault);
        }


        /// <summary>
        /// Binds a property to a cloned instance of IEnumerable
        /// </summary>
        /// <param name="identifiableParam"></param>
        /// <param name="pi"></param>
        /// <returns></returns>
        private static MemberBinding GetEnumerableBinding(UnaryExpression identifiableParam, PropertyInfo pi, ParameterExpression repoParam)
        {
            var type = pi.PropertyType;
            var cloneArray = typeof(IdentifiableCopier).GetTypeInfo().GetDeclaredMethod("CloneEnumerable").MakeGenericMethod(type.GetTypeInfo().GenericTypeArguments[0]);
            var cloneArrayParam = Expression.Parameter(typeof(IEnumerable));
            var cloneCall = Expression.Call(null,
                cloneArray,
                cloneArrayParam,
                repoParam);
            var cloneLambda = Expression.Lambda(cloneCall, cloneArrayParam, repoParam);
            var cloneInvoke = Expression.Invoke(cloneLambda, Expression.MakeMemberAccess(identifiableParam, pi), repoParam);
            return Expression.Bind(pi, Expression.Convert(cloneInvoke, type));
        }

        /// <summary>
        /// Binds a property to a cloned instance of IIdentifiable[]
        /// </summary>
        /// <param name="identifiableParam"></param>
        /// <param name="pi"></param>
        /// <returns></returns>
        private static MemberBinding GetArrayBinding(UnaryExpression identifiableParam, PropertyInfo pi, ParameterExpression repoParam)
        {
            var type = pi.PropertyType;
            var cloneArray = typeof(IdentifiableCopier).GetTypeInfo().GetDeclaredMethod("CloneArray").MakeGenericMethod(type.GetTypeInfo().GetElementType());
            var cloneArrayParam = Expression.Parameter(typeof(Array));
            var cloneCall = Expression.Call(null,
                cloneArray,
                cloneArrayParam,
                repoParam);
            var cloneLambda = Expression.Lambda(cloneCall, cloneArrayParam, repoParam);
            var cloneInvoke = Expression.Invoke(cloneLambda, Expression.MakeMemberAccess(identifiableParam, pi), repoParam);
            return Expression.Bind(pi, Expression.Convert(cloneInvoke, type));
        }

        private static IEnumerable CloneEnumerable<U>(IEnumerable list, IRepository repository) where U : class
        {
            if (list == null) return null;

            if (list is Array) return CloneArray<U>((Array)list, repository);
            if (list is IList) return CloneList<U>((IList)list, repository);

            var newList = new List<U>();

            var en = list.GetEnumerator();
            while (en.MoveNext())
            {
                var item = (IIdentifiable)en.Current;
                if (item == null)
                {
                    newList.Add((U)null);
                }
                else
                {
                    var cloner = GetCloner(item.GetType(), repository);
                    newList.Add(cloner(repository, item) as U);
                }
            }
            return newList.AsEnumerable();
        }

        private static Array CloneArray<U>(Array array, IRepository repository) where U : class
        {
            if (array == null) return null;

            var clone = Array.CreateInstance(typeof(U), array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);
                if (item == null)
                {
                    clone.SetValue(null, i);
                }
                else
                {
                    var cloner = GetCloner(item.GetType(), repository);
                    clone.SetValue(cloner(repository, (IIdentifiable)item), i);
                }
            }
            return clone;
        }

        private static IList CloneList<U>(IList list, IRepository repository) where U : class
        {
            if (list == null) return null;

            var clone = Activator.CreateInstance(list.GetType()) as IList;

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item == null)
                {
                    clone.Add(null);
                }
                else
                {
                    var cloner = GetCloner(item.GetType(), repository);
                    clone.Add(cloner(repository, (IIdentifiable)item));
                }
            }
            return clone;
        }

        class IdentifiableFunctions
        {
            public Func<IRepository, IIdentifiable, IIdentifiable> Clone { get; set; }
            public Action<IIdentifiable, IIdentifiable> Copy { get; set; }
        }
    }


}
