using SHS.Sage.Linq;
using SHS.Sage.Linq.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SHS.Sage.UnitOfWork;

namespace SHS.Sage
{
    public partial class ORepository
    {
        protected virtual int DeleteInternal(string id)
        {
            return DeleteInternal((IIdentifiable)new _Thing() { Id = id });
        }

        protected virtual int DeleteInternal(IIdentifiable identity)
        {
            return DeleteInternal(CreateIdentifiableSet(identity), identity, null);
        }

        protected virtual int DeleteInternal<T>(T identity) where T : IIdentifiable
        {
            return DeleteInternal(CreateIdentifiableSet<T>(identity), identity, null);
        }

        protected virtual int DeleteInternal(IQueryable queryable, IIdentifiable identity, Expression<Func<IIdentifiable, bool>> deleteCheck)
        {
            var genericDeleteInternal = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(mi => mi.Name.Equals("DeleteInternal") && mi.IsGenericMethod && mi.GetParameters().Length == 3)
                .MakeGenericMethod(queryable.GetType().GetGenericArguments()[0]);
            return (int)genericDeleteInternal.Invoke(this, new object[] { queryable, identity, deleteCheck });
        }

        protected virtual int DeleteInternal<T>(IQueryable<T> queryable, T identity, Expression<Func<T, bool>> deleteCheck) where T : IIdentifiable
        {
            var identifiable = identity is IProxyIdentifiable
                ? (Expression)Expression.Convert(Expression.Constant(identity), ((IProxyIdentifiable)identity).IdentifiableType)
                : Expression.Constant(identity);

            var callMyself = Expression.Call(
                Expression.Constant(this),
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                Expression.Convert(queryable.Expression, typeof(IQueryable<T>)),
                identifiable,
                deleteCheck != null ? (Expression)Expression.Quote(deleteCheck) : Expression.Constant(null, typeof(Expression<Func<T, bool>>))
                );
            return (int)queryable.Provider.Execute(callMyself);
        }

        protected virtual IIdentifiableSet<T> CreateIdentifiableSet<T>(T identity) where T : IIdentifiable
        {
            if (identity is IThing)
            {
                var thingSetType = typeof(OThingSet<>).MakeGenericType(typeof(T));
                return (IIdentifiableSet<T>)Activator.CreateInstance(thingSetType, new object[] { new OTrackingQueryProvider(this), this });
            }
            else throw new NotImplementedException();
        }

        protected virtual IIdentifiableSet CreateIdentifiableSet(IIdentifiable identity)
        {
            if (identity is IThing)
            {
                var type = GetIdentifiableType(identity.GetType());

                var thingSetType = typeof(OThingSet<>).MakeGenericType(type);
                return (IIdentifiableSet)Activator.CreateInstance(thingSetType, new object[] { new OTrackingQueryProvider(this), this });
            }
            else
            {
                var type = GetIdentifiableType(identity.GetType());

                var thingSetType = typeof(IdentifiableSet<>).MakeGenericType(type);
                return (IIdentifiableSet)Activator.CreateInstance(thingSetType, new object[] { new OTrackingQueryProvider(this), this, null });
            }
        }

        protected virtual Type GetIdentifiableType(Type sourceType)
        {
            return sourceType.Equals(typeof(_Thing))
                ? typeof(IIdentifiable)
                : sourceType.Implements(typeof(IProxyIdentifiable))
                    ? sourceType.BaseType
                    : sourceType;
        }

        protected virtual IIdentifiable InsertInternal(IQueryable queryable, IIdentifiable identity, Expression<Func<IIdentifiable, IIdentifiable>> resultSelector)
        {
            var identifiable = identity is IProxyIdentifiable
                ? (Expression)Expression.Convert(Expression.Constant(identity), ((IProxyIdentifiable)identity).IdentifiableType)
                : Expression.Constant(identity);

            var callMyself = Expression.Call(
                Expression.Constant(this),
                (MethodInfo)MethodInfo.GetCurrentMethod(),
                queryable.Expression,
                identifiable,
                Expression.Constant(resultSelector, typeof(Expression<Func<IIdentifiable, IIdentifiable>>))
            );
            return (IIdentifiable)new OTrackingQueryProvider(this).Execute(callMyself);
        }

        protected virtual IIdentifiable UpdateInternal(IIdentifiableSet queryable, IIdentifiable identity, Expression<Func<IIdentifiable, bool>> updateCheck, Expression<Func<IIdentifiable, IIdentifiable>> resultSelector)
        {
            var identifiable = identity is IProxyIdentifiable
                ? (Expression)Expression.Convert(Expression.Constant(identity), ((IProxyIdentifiable)identity).IdentifiableType)
                : Expression.Constant(identity);

            var callMyself = Expression.Call(
                       Expression.Constant(this),
                       (MethodInfo)MethodInfo.GetCurrentMethod(),
                       queryable.Expression,
                       identifiable,
                       Expression.Constant(updateCheck, typeof(Expression<Func<IIdentifiable, bool>>)),
                       Expression.Constant(resultSelector, typeof(Expression<Func<IIdentifiable, IIdentifiable>>))
                       );
            return (IIdentifiable)new OTrackingQueryProvider(this).Execute(callMyself);
        }

#if (DEBUG)
        List<TrackedOperation> _operations = new List<TrackedOperation>();
#endif
        protected virtual ITrackingRepository SaveChangesInternal()
        {
            // the general algorithm is as follows:
            // 0. update tracking states
            // 1. apply all deletes
            // 2. apply all inserts
            // 3. apply all updates
            //
            // The one complexity comes in step 2 when there exists co-dependent inserts in the change set
            // the simplest example would be given two types, A and B, where A has a property of type B
            // and B has a property of type A, and both are being inserted in the same change set.
            // 
            // To handle this situation, the insert process needs to detect and ignore dependent inserts, 
            // and allow them to be inserted discretely.  After insert, a new Id will be assigned, which should 
            // put the parent items which have already been inserted back into a ShouldSave state, and applied by step 3. 
            // For items not yet inserted, the dependent item that has been inserted won't be ignored, as it will now 
            // have an Id.
#if(DEBUG)
            _operations.Clear();
#endif
            foreach(var instance in this.ToList())
            {
                // capture any untracked items that may have been instantiated and attached to a tracked instance
                Cascade(instance.Identifiable, (item) => Attach(item));
            }

            _tracking.CalculateStates();
            
            var changes = _tracking.Where(t => t.State != TrackingState.IsUnchanged && t.State != TrackingState.IsNotTracked)
                .GroupBy(t => t.State);

            foreach(var changeGroup in changes)
            {
                if (changeGroup.Key == TrackingState.ShouldDelete)
                {
                    ApplyDeletes(changeGroup);
                }
                else if (changeGroup.Key == TrackingState.ShouldSave)
                {
                    ApplySaves(changeGroup);
                }
            }

            this.AcceptChanges();

            return this;
        }

        private void ApplySaves(IGrouping<TrackingState, ITrackedIdentifiable> changeGroup)
        {
            // we need Things to insert before Associations - we can't insert nulls into complex association properties
            var inserts = changeGroup.Where(ti => string.IsNullOrEmpty(ti.Identifiable.Id))
                                     .OrderBy(ti => ti.Identifiable is IThing ? 0 : 1)
                                     .ToArray();
            var updates = changeGroup.Where(ti => !string.IsNullOrEmpty(ti.Identifiable.Id) && ti.State == TrackingState.ShouldSave)
                                     .OrderBy(ti => ti.Identifiable is IThing ? 0 : 1)
                                     .ToArray();
            var done = false;
            do
            {
                ApplyInserts(inserts);
                ApplyUpdates(updates);
                _tracking.CalculateStates();
                inserts = changeGroup.Where(ti => string.IsNullOrEmpty(ti.Identifiable.Id)).ToArray();
                updates = changeGroup.Where(ti => !string.IsNullOrEmpty(ti.Identifiable.Id) && ti.State == TrackingState.ShouldSave).ToArray();
                done = inserts.Length + updates.Length == 0;
            } while (!done);

        }

        private void ApplyInserts(IEnumerable<ITrackedIdentifiable> inserts)
        {
            _tracking.IsTracking = false; // need to turn this off to prevent new items with new Ids being added
            foreach (var tracked in inserts)
            {
                tracked.ResetState(InsertInternal(CreateIdentifiableSet(tracked.Identifiable), tracked.Identifiable, t => t));
#if (DEBUG)
                _operations.Add(new TrackedOperation(tracked, OperationType.Insert));
#endif
            }
            _tracking.IsTracking = true;

        }

        private void ApplyUpdates(IEnumerable<ITrackedIdentifiable> updates)
        {
            foreach (var tracked in updates)
            {
                tracked.ResetState(UpdateInternal(CreateIdentifiableSet(tracked.Identifiable), tracked.Identifiable, null, t => t));
#if (DEBUG)
                _operations.Add(new TrackedOperation(tracked, OperationType.Update));
#endif
            }
        }

        private void ApplyDeletes(IGrouping<TrackingState, ITrackedIdentifiable> changeGroup)
        {
            foreach(var delete in changeGroup.ToArray())
            {
                var count = this.DeleteInternal(delete.Identifiable);
#if (DEBUG)
                _operations.Add(new TrackedOperation(delete, OperationType.Delete));
#endif
                if (count == 1)
                {
                    _tracking.Remove(delete.Identifiable);
                    delete.Identifiable.Id = "#Deleted";
                }
            }
        }
#if(DEBUG)
        public IEnumerable<TrackedOperation> Operations
        {
            get
            {
                return _operations.AsEnumerable();
            }
        }
#endif
    }
}
