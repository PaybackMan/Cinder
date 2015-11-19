using Orient.Client;
using SHS.Sage.Linq;
using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Policy;
using SHS.Sage.Linq.Runtime;
using SHS.Sage.Linq.UnitOfWork;
using SHS.Sage.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SHS.Sage.UnitOfWork;
using SHS.Sage.Linq.Mapping;
using SHS.Sage.Mapping;

namespace SHS.Sage
{
    public partial class ORepository : ITrackingRepository
    {
        TrackingManager _tracking;
        static ORepository()
        {
            CacheProvider.Register<ORepository, QueryCache>();
        }

        public ORepository(ODatabase connection)
        {
            this.Connection = connection;
            this.Policy = new OQueryPolicy();
           
            var mappingSystem = OTypeMapping.Mappings(this);
            if (!mappingSystem.IsInitialized)
            {
                var fluentMapper = new OFluentMapper(this.GetType());
                this.OnRegisterIdentifiables(fluentMapper); // get mappings from sub type
                var discovered = DiscoverIdentifiableTypes(fluentMapper).ToArray(); // get default mappings
                var entities = fluentMapper.Entities
                    .Union(discovered.Where(e => !fluentMapper.IsMapped(e.EntityType)))
                    .ToArray(); // union explicit and implicit mappings

                foreach (var mapping in entities)
                {
                    mappingSystem.AddTypeMapping(mapping);
                }

                mappingSystem.AddTypeMapping(fluentMapper.AutoMap(typeof(_Thing)));
                mappingSystem.AddTypeMapping(fluentMapper.AutoMap(typeof(_Association)));
                mappingSystem.AddTypeMapping(fluentMapper.AutoMap(typeof(_Actor)));

                OTypeBuilder.PreBuildTypes(new OQueryLinguist(new OQueryLanguage(), null), mappingSystem);
                mappingSystem.IsInitialized = true;
            }
            _tracking = new TrackingManager(this);

            var repoType = this.GetType();
            SchemaBuilder = OSchemaBuilder.Get(this.GetType());
            SchemaBuilder.BuildSchema(this);
        }

        #region IRepository
        /// <summary>
        /// Gets an instance of the database connection used by this repository
        /// </summary>
        public ODatabase Connection { get; private set; }
        /// <summary>
        /// Gets an instance of the Query Policy used by this repository instance
        /// </summary>
        public IQueryPolicy Policy { get; protected set; }
        /// <summary>
        /// Gets an instance of the type mapping system
        /// </summary>
        public IMapEntities Mapping { get { return OTypeMapping.Mappings(this); } }
        /// <summary>
        /// Gets an instance of the schema builder to use for this repository type
        /// </summary>
        public IBuildSchema SchemaBuilder { get; private set; }
        /// <summary>
        /// Gets a boolean indicating whether the state of this repository is valid for use
        /// </summary>
        public bool IsValid { get { return !_disposed; } }
        /// <summary>
        /// Gets an instance of the Query Cache used to process all queries
        /// </summary>
        public ICacheQueries QueryCache
        {
            get
            {
                return CacheProvider.Get<ORepository>();
            }
        }
        /// <summary>
        /// Returns a new instance of an ActivitySet of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IActivitySet<T> ActivitySet<T>() where T : IActivity
        {
            return new OActivitySet<T>(new OTrackingQueryProvider(this), this);
        }
        /// <summary>
        /// Returns a new instance of an AssociationSet of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IAssociationSet<T> AssociationSet<T>() where T : IAssociation
        {
            return new OAssociationSet<T>(new OTrackingQueryProvider(this), this);
        }
        /// <summary>
        /// Returns a new instance of an ThingSet of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IThingSet<T> ThingSet<T>() where T : IThing
        {
            return new OThingSet<T>(new OTrackingQueryProvider(this), this);
        }
        /// <summary>
        /// Gets a populated instance of identity of type T from the provider, unless a matching instance 
        /// is found in the Tracking Manager, in which case, the current tracked instance is returned, without 
        /// querying the storage provider.  In the event that the item is being tracked, and its TrackingState 
        /// equals ShouldDelete, an InvalidOperationException will be thrown indicating that the item has been 
        /// submitted for deletion.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <returns></returns>
        public T Get<T>(T identity) where T : IIdentifiable
        {
            var tracked = _tracking.Get(identity);
            if (tracked == null || tracked.State == TrackingState.IsNotTracked)
            {
                return new DataSet<T>(new OTrackingQueryProvider(this), this, null).Single<T>(t => t.Id == identity.Id);
            }
            else if (tracked.State == TrackingState.ShouldDelete && !Policy.ReturnTrackedDeletes)
            {
                throw new InvalidOperationException("The item has been submitted for deletion.");
            }
            else
            {
                return (T)tracked.Identifiable;
            }
        }
        /// <summary>
        /// Gets a populated instance of identity of type T from the provider, unless a matching instance 
        /// is found in the Tracking Manager, in which case, the current tracked instance is returned, without 
        /// querying the storage provider.  In the event that the item is being tracked, and its TrackingState 
        /// equals ShouldDelete, an InvalidOperationException will be thrown indicating that the item has been 
        /// submitted for deletion.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public IIdentifiable Get(IIdentifiable identity)
        {
            var type = GetIdentifiableType(identity.GetType());
            var func = typeof(Func<,>).MakeGenericType(type, type);
            var method = this.GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Single(m => m.Name.Equals("Get") && m.IsGenericMethod && m.GetParameters()[0].ParameterType != typeof(string));
            method = method.MakeGenericMethod(type);
            var identityParameter = Expression.Parameter(identity.GetType());
            var del = Expression.Lambda(Expression.Call(Expression.Constant(this), method, identityParameter), identityParameter).Compile();
            return (IIdentifiable)del.DynamicInvoke(identity);
        }
        /// <summary>
        /// Gets a populated instance for the given Id from the provider, unless a matching instance 
        /// is found in the Tracking Manager, in which case, the current tracked instance is returned, without 
        /// querying the storage provider.  In the event that the item is being tracked, and its TrackingState 
        /// equals ShouldDelete, an InvalidOperationException will be thrown indicating that the item has been 
        /// submitted for deletion.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IIdentifiable Get(string id)
        {
            var oClass = SchemaBuilder.Schema.GetClass(id);
            if (oClass.ClassType == ClassType.Thing)
            {
                return Get((IThing)(new _Thing() { Id = id }));
            }
            else
            {
                return Get((IAssociation)(new _Association() { Id = id }));
            }
        }
        /// <summary>
        /// Gets a populated instance for the given Id from the provider, unless a matching instance 
        /// is found in the Tracking Manager, in which case, the current tracked instance is returned, without 
        /// querying the storage provider.  In the event that the item is being tracked, and its TrackingState 
        /// equals ShouldDelete, an InvalidOperationException will be thrown indicating that the item has been 
        /// submitted for deletion.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get<T>(string id) where T : IIdentifiable
        {
            var identifiable = Activator.CreateInstance<T>();
            identifiable.Id = id;
            return Get<T>(identifiable);
        }
        /// <summary>
        /// Executes a manually constructed query against the underlying storage provider without returing any result.
        /// </summary>
        /// <param name="query"></param>
        public virtual void ExecuteStatement(string query)
        {
            new OTrackingQueryProvider(this).ExecuteStatement(query);
        }
        /// <summary>
        /// Executes a manually constructed query against the underlying storage provider, returning an IReadData instance 
        /// to enumerate records and fields in the result set.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual IReadData ExecuteReader(string query)
        {
            return new OTrackingQueryProvider(this).ExecuteReader(query);
        }

        /// <summary>
        /// Executes a raw query against the data store. To take advantage of subclassed results, be sure to 
        /// include "@class" in your select projection, as the type build system will key off this value 
        /// to determine which concrete type to marshal your results into.  Without that specifier, then all results 
        /// will be marshalled into type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> ExecuteEnumerable<T>(string query) where T : IIdentifiable
        {
            query = query.Replace("@class", "@class as +class");
            return new OTrackingQueryProvider(this).ExecuteEnumerable<T>(query);
        }

        private IEnumerable<IMappedEntity> DiscoverIdentifiableTypes(OFluentMapper mapper)
        {
            foreach(var mi in this.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.MemberType == MemberTypes.Property && TypeHelper.GetMemberType(m).Implements(typeof(IIdentifiableSet<>))))
            {
                var type = TypeHelper.GetMemberType(mi).GetGenericArguments()[0];
                yield return mapper.AutoMap(type); // need to make this recursive
            }
        }

        protected virtual void OnRegisterIdentifiables(IFluentMapper mapper)
        {
        }

        #endregion

        #region ITrackingRepository
        /// <summary>
        /// Removes an item from the Tracking Manager.  The item will not be consider in subsequent calls to SaveChanges, unless it 
        /// is added to the Tracking Manager with a subsequent call to Attach.  If cascade is True, this will walk the instance graph 
        /// looking for loaded IIdentifiables to detach.  This call will not load Deferred Loaded instances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifiable"></param>
        /// <returns></returns>
        public T Detach<T>(T identifiable, bool cascade = false) where T : IIdentifiable
        {
            var instance = _tracking.Remove<T>(identifiable);
            if (cascade)
            {
                var currentDeferredLoad = Policy.DeferLoadComplexTypes;
                Policy.DeferLoadComplexTypes = false;
                Cascade(identifiable, (item) => Detach(item, true));
                Policy.DeferLoadComplexTypes = currentDeferredLoad;
            }
            return instance;
        }

        /// <summary>
        /// Gets the tracking state for a given item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns>the state of the tracked item.  If the item is not being tracked, TrackingState.Untracked will be returned.</returns>
        public TrackingState GetState<T>(T identifiable) 
        {
            var tracked = _tracking.Get(identifiable as IIdentifiable);
            if (tracked == null) return TrackingState.IsNotTracked;
            else return tracked.State;
        }

        /// <summary>
        /// Instantiates a new tracked instance of T.  The TrackingState given the new instance will depend on a combination 
        /// of variables including whether the new instance specifies a memberInitializer setting the Id property of the new instance, 
        /// and whether a similarly identified instance is already being tracked in a conflicting state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInitializer">an optional Action<T> delegate to call to initialize any property/field members</param>
        /// <returns>the newly created instance of T</returns>
        public T Create<T>(Action<T> memberInitializer = null) where T : IIdentifiable, new()
        {
            if (Policy.TrackChanges)
            {
                var entity = new T(); 
                if (memberInitializer != null)
                {
                    memberInitializer(entity);
                }
                return Attach(entity, TrackingState.ShouldSave); // TrackingManager should clone this and return a proxy
            }
            else throw new InvalidOperationException("A new tracked instance cannot be created when Policy.TrackChanges equals False.");
        }

        /// <summary>
        /// Instantiates a new tracked instance of T.  The TrackingState given the new instance will depend on a combination 
        /// of variables including whether the new instance specifies a memberInitializer setting the Id property of the new instance, 
        /// and whether a similarly identified instance is already being tracked in a conflicting state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initializer">a Func<T> to invoke to create the new instance</T>/></param>
        /// <returns>a tracked instance of T</returns>
        public T Create<T>(Func<T> initializer) where T : IIdentifiable, new()
        {
            if (Policy.TrackChanges)
            {
                var entity = initializer();
                return Attach(entity, TrackingState.ShouldSave); // TrackingManager should clone this and return a proxy
            }
            else throw new InvalidOperationException("A new tracked instance cannot be created when Policy.TrackChanges equals False.");
        }

        /// <summary>
        /// Adds the identifiable instance to repository in the provided TrackingState, if possible.  The provided instance will be wrapped 
        /// in a runtime equivalent proxy, which should be used for all subsequent calls and state changes.  The original instance provided 
        /// will not be tracked or maintained by the state tracking system.  If you wish to create a new tracked instance of T, consider using the 
        /// Create<T>() method, which will construct and return a populated, tracked instance of T.  If Policy.TrackChanges is False, 
        /// the instance will still be proxied, but it will not be added to the Tracking Manager.  Changing Policy.TrackChanges to True, 
        /// and submitting the returned instance to Attach again will then add the item to the Tracking Manager.  The TrackingState submitted 
        /// is not guaranteed to be applied, depending on the current state of the Tracking Manager. Remarks for information on TrackingState.
        /// </summary>
        /// <typeparam name="T">the Type of identifiable to attach</typeparam>
        /// <param name="identifiable">the instance to attach</param>
        /// <param name="state">the desired TrackingState to attach with - note the provided state may be changed by the provider</param>
        /// <returns>an instance of the attached entity</returns>
        /// <remarks>In some cases, the provided state may conflict if the instance provided is already being tracked in an incompatible state.  E.g. 
        /// if the instance provided includes a populated Id property, and the state = TrackingState.Added, the state will be changed to Modified.</remarks>
        public T Attach<T>(T identifiable, TrackingState state = TrackingState.Unknown)
        {
            if (identifiable is IIdentifiable)
            {
                var tracked = _tracking.Get(((IIdentifiable)identifiable).Id);
                if (tracked == null)
                {
                    if (!(identifiable is IProxyIdentifiable))
                    {
                        var copier = new ProxiedIdentifiableCopier(this);
                        identifiable = (T)copier.Copy((IIdentifiable)identifiable); // we want to proxy this when it's attached
                    }
                    tracked = _tracking.Track<T>(identifiable, state);
                }
                else
                {
                    _tracking.SetState(tracked, state);
                }

                if (tracked == null) return identifiable;
                else return (T)tracked.Identifiable;
            }
            else return identifiable;
        }

        public void ClearChanges()
        {
            _tracking.Clear();
#if (DEBUG)
            _operations.Clear();
#endif
        }

        /// <summary>
        /// Puts all currently tracked items into TrackingState.Unkown state, as if any pending changes have been applied.
        /// </summary>
        public void AcceptChanges()
        {
            foreach (var tracked in _tracking)
                tracked.ResetState();
        }

        /// <summary>
        /// Marks the supplied item for deletion, if the Policy.TrackChanges setting is currently True, otherwise, throws an exception
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="cascade">true to recurse the property graph and mark all child IIdentifiables (including enumerables) for deletion.  
        /// This operation will load any defer loaded properties it discovers as it traverses, if the current policy is set to allow deferred loading.</param>
        /// <returns></returns>
        public IIdentifiable Delete(IIdentifiable identity, bool cascade = false)
        {
            if (identity == null) return identity;

            var type = identity.GetType().Equals(typeof(_Thing)) ? typeof(IIdentifiable) : identity.GetType();

            var method = this.GetType().GetMethods()
                .Single(m => m.IsGenericMethod && m.Name.Equals("Delete"))
                .MakeGenericMethod(type);

            var p = Expression.Parameter(identity.GetType(), "identity");
            var c = Expression.Parameter(typeof(bool), "cascade");

            try
            {
                return (IIdentifiable)Expression.Lambda(
                            Expression.Call(Expression.Constant(this), method, p, c),
                        p, c).Compile().DynamicInvoke(identity, cascade);
            }
            catch(TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// Marks the supplied item for deletion, if the Policy.TrackChanges setting is currently True, otherwise, throws an exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="cascade">true to recurse the property graph and mark all child IIdentifiables (including enumerables) for deletion.  
        /// This operation will load any defer loaded properties it discovers as it traverses, if the current Policy.DeferLoadComplexTypes is True.</param>
        /// <returns></returns>
        public T Delete<T>(T identity, bool cascade = false) where T : IIdentifiable
        {
            if (identity == null) return identity;

            if (this.Policy.TrackChanges)
            {
                var tracked = _tracking.Track(identity, TrackingState.ShouldDelete);
                if (cascade)
                {
                    Cascade(identity, (item) => Delete(item, true));
                }
                if (tracked == null) return default(T);
                else return (T)tracked.Identifiable;
            }
            else throw new InvalidOperationException("The repository cannot process changes when the Policy.TrackChanges setting is set to False.");
        }


        private void Cascade(IIdentifiable instance, Action<IIdentifiable> action)
        {
            if (instance == null) return;

            foreach(var pi in instance.GetType().GetProperties().Where(p => p.CanRead && p.CanWrite))
            {
                if (pi.PropertyType.Implements(typeof(IIdentifiable)))
                {
                    action((IIdentifiable)pi.GetValue(instance));
                }
                else if (pi.PropertyType.Implements(typeof(IEnumerable)))
                {
                    var en = pi.GetValue(instance) as IEnumerable;
                    if (en != null)
                    {
                        var enumerator = en.GetEnumerator();
                        while(enumerator.MoveNext())
                        {
                            var identifiable = enumerator.Current as IIdentifiable;
                            if (identifiable != null)
                            {
                                action(identifiable);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Marks the supplied item for deletion, if the Policy.TrackChanges setting is currently True, otherwise, throws an exception
        /// </summary>
        /// <param name="id">the Id of the item to delete</param>
        /// <returns></returns>
        public IIdentifiable Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("Id cannot be null");
            }
            var oClass = SchemaBuilder.Schema.GetClass(id);
            if (oClass.ClassType == ClassType.Thing)
            {
                return Delete((IThing)(new _Thing() { Id = id }));
            }
            else
            {
                return Delete((IAssociation)(new _Association() { Id = id }));
            }
        }

        /// <summary>
        /// Processes all tracked changes, and attempts to commit them to the underlying data store.
        /// </summary>
        /// <returns>an instance of this repository for simple call chaining</returns>
        public ITrackingRepository SaveChanges()
        {
            return SaveChangesInternal();
        }

        /// <summary>
        /// Gets an enumerator for all the currently tracked changes in this repository instance
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ITrackedIdentifiable> GetEnumerator()
        {
            return _tracking.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IDisposable
        bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // release managed references here
                this.Connection.Dispose();
                this.Connection = null;
                this._tracking.Clear();
                this._tracking = null;
                this.SchemaBuilder = null;
                this.OnDisposeManaged();
            }

            // release unmanged references here
            this.OnDisposeUnmanaged();

            _disposed = true;
        }

        /// <summary>
        /// Closes the storage connection and disposes of all tracked items
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDisposeManaged()
        {

        }

        protected virtual void OnDisposeUnmanaged()
        {

        }

        #endregion
    }
}
