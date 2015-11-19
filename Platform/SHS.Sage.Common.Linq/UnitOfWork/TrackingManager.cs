using SHS.Sage.Linq;
using SHS.Sage.Linq.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.UnitOfWork
{
    public class TrackingManager : IEnumerable<ITrackedIdentifiable>
    {
        Dictionary<TrackedKey, ITrackedIdentifiable> _cached = new Dictionary<TrackedKey, ITrackedIdentifiable>(new TrackedKeyComparer());

        public TrackingManager(IRepository repository)
        {
            this.Repository = repository;
            this.IsTracking = repository.Policy.TrackChanges;
        }

        public IRepository Repository { get; private set; }

        public bool IsTracking { get; set; }

        /// <summary>
        /// Adds/updates an instance to the cache, and attempts to set the state, if possible.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ITrackedIdentifiable Track<T>(T instance, TrackingState state = TrackingState.Unknown)
        {
            if (!IsTracking || !Repository.Policy.TrackChanges) return null;
            if ((instance as IIdentifiable) == null) return null;

            ITrackedIdentifiable tracked = null;
            var key = new TrackedKey(instance as IIdentifiable);
            bool exists = _cached.TryGetValue(key, out tracked);

            state = GetAdjustedState(tracked, instance as IIdentifiable, state);

            if (!exists && state == TrackingState.IsNotTracked)
            {
                return null;
            }
            else if (!exists)
            {
                // new entry
                tracked = Create<T>(instance, Repository, state);
                _cached[key] = tracked;
            }
            else if (state != TrackingState.IsNotTracked)
            {
                // existing entry
                if (state != TrackingState.ShouldDelete)
                {
                    if (tracked.State == TrackingState.Unknown)
                    {
                        tracked.CalculateState();
                    }
                    if (tracked.State == TrackingState.IsUnchanged)
                    {
                        tracked.ResetState((IIdentifiable)instance); // we have no local changes, so update the reference to use the new values
                    }
                }
                tracked.State = state;
            }
            else if (state == TrackingState.IsNotTracked)
            {
                if (exists)
                {
                    Remove<T>(instance);
                }
                return null;
            }
            return tracked;
        }

        public TrackingState SetState(ITrackedIdentifiable tracked, TrackingState state)
        {
            if (state == TrackingState.IsNotTracked)
            {
                Remove(tracked.Identifiable);
                return TrackingState.IsNotTracked;
            }
            else
            {
                tracked.State = GetAdjustedState(tracked, tracked.Identifiable, state);
            }
            return tracked.State;
        }

        private ITrackedIdentifiable Create<T>(T instance, IRepository repository, TrackingState state)
        {
            var type = typeof(T);
            if (type == typeof(IIdentifiable))
            {
                type = instance.GetType();
                if (instance is IProxyIdentifiable)
                {
                    type = type.BaseType;
                }
                var trackedType = typeof(TrackedIdentifiable<>).MakeGenericType(type);
                return (TrackedIdentifiable)Activator.CreateInstance(trackedType, new object[] { instance, repository, new ProxiedIdentifiableCopier(this.Repository), state });
            }
            else
            {
                return new TrackedIdentifiable<T>(instance, repository, new ProxiedIdentifiableCopier(this.Repository), state);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////
        //              | Desired
        //===============================================================================================
        // Current      | New       | Modified  | Deleted           | Unknown   | Unchanged | NotTracked
        //-----------------------------------------------------------------------------------------------
        // Modified     |Modified   |Modified   |Deleted/NotTracked |Modified   |Unchanged  |NotTracked
        // Deleted      |Deleted    |Deleted    |Deleted            |Deleted    |Unchanged  |NotTracked
        // Unknown      |Unknown    |Modified   |Deleted            |Unknown    |Unchanged  |NotTracked
        // Unchanged    |Unchanged  |Modified   |Deleted            |Unknown    |Unchanged  |NotTracked
        // NotTracked   |NotTracked |Modified   |Deleted            |Unknown    |Unchanged  |NotTracked
        //===============================================================================================
        private TrackingState GetAdjustedState(ITrackedIdentifiable tracked, IIdentifiable instance, TrackingState desiredState)
        {
            if (desiredState == TrackingState.IsNotTracked)
                return TrackingState.IsNotTracked;

            if (tracked == null)
            {
                // this a new entry, just take whatever state is provided
                return desiredState;
            }
            if (desiredState == TrackingState.IsUnchanged)
            {
                return TrackingState.IsUnchanged;
            }
            else if (tracked.State == TrackingState.ShouldDelete && desiredState != TrackingState.IsNotTracked)
            {
                return TrackingState.ShouldDelete;
            }
            else if (tracked.State == TrackingState.ShouldSave
                 && (desiredState == TrackingState.ShouldSave || desiredState == TrackingState.Unknown))
            {
                return TrackingState.ShouldSave;
            }
            else if (tracked.State == TrackingState.ShouldSave && desiredState == TrackingState.ShouldDelete)
            {
                return string.IsNullOrEmpty(tracked.Identifiable.Id) ? TrackingState.IsNotTracked : TrackingState.ShouldDelete;
            }
            else if (tracked.State == TrackingState.ShouldSave && desiredState == TrackingState.IsUnchanged)
            {
                return TrackingState.IsUnchanged;
            }
            else if (tracked.State == TrackingState.Unknown || tracked.State == TrackingState.IsUnchanged)
            {
                return desiredState;
            }
            else
                return desiredState;
        }

        /// <summary>
        /// Clears the tracking cache
        /// </summary>
        public void Clear()
        {
            _cached.Clear();
        }

        /// <summary>
        /// Removes the tracked instance from the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public T Remove<T>(T instance)
        {
            var key = new TrackedKey(instance as IIdentifiable);
            _cached.Remove(key);
            return instance;
        }

        /// <summary>
        /// Returns true if the instance is being tracked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Contains<T>(T instance)
        {
            return _cached.ContainsKey(new TrackedKey(instance as IIdentifiable));
        }

        /// <summary>
        /// Gets a TrackedIdentifiable, if it exists, otherwise, returns null.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public ITrackedIdentifiable Get(IIdentifiable instance)
        {
            ITrackedIdentifiable item;
            if (instance is IIdentifiable
                && _cached.TryGetValue(new TrackedKey(instance as IIdentifiable), out item))
            {
                return item;
            }
            return null;
        }

        /// <summary>
        /// Gets a TrackedIdentifiable by its Id, if it exists, otherwise, returns null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ITrackedIdentifiable Get(string id)
        {
            return Get(new _Thing() { Id = id });
        }

        /// <summary>
        /// Computes all tracked item states
        /// </summary>
        public void CalculateStates()
        {
            foreach (var item in _cached.Values.ToArray())
                item.CalculateState();
        }

        /// <summary>
        /// Enumerates the current tracking cache
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ITrackedIdentifiable> GetEnumerator()
        {
            foreach (var item in _cached.Values)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class TrackedKey : IEquatable<IIdentifiable>
        {
            public TrackedKey(IIdentifiable identifiable)
            {
                this.Identifiable = identifiable;
            }

            public IIdentifiable Identifiable { get; private set; }

            public bool Equals(IIdentifiable other)
            {
                return this.Identifiable != null 
                    && other != null
                    && (this.Identifiable == other 
                    || (!string.IsNullOrEmpty(this.Identifiable.Id) && this.Identifiable.Id.Equals(other.Id)));
            }

            public override bool Equals(object obj)
            {
                if (obj is TrackedKey)
                {
                    return Equals(((TrackedKey)obj).Identifiable);
                }
                else return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return 0; // because our Ids can change after inserts, we need all our items to sit in the same bucket to force the dictionary to match them after their Ids change
            }
        }

        class TrackedKeyComparer : IEqualityComparer<TrackedKey>, IEqualityComparer
        {
            public bool Equals(TrackedKey x, TrackedKey y)
            {
                return (x == null && y == null)
                    || (
                    x != null && y != null
                    && ((TrackedKey)x).Equals((TrackedKey)y));
            }

            public new bool Equals(object x, object y)
            {
                return Equals(x as TrackedKey, y as TrackedKey);
            }

            public int GetHashCode(TrackedKey obj)
            {
                return obj.GetHashCode();
            }

            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
