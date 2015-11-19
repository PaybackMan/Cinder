using SHS.Sage.Linq.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHS.Sage.UnitOfWork
{
    public interface ITrackedIdentifiable<T> : ITrackedIdentifiable
    {
        new T Identifiable { get; }
    }

    public interface ITrackedIdentifiable : IDisposable
    {
        IIdentifiable Identifiable { get; }
        TrackingState State { get; set; }
        bool IsObserving { get; }
        IRepository Repository { get; }
        TrackingState CalculateState(bool forceEvaluation = false);
        void ResetState();
        void ResetState(IIdentifiable instance);
    }

    /// <summary>
    /// Simple class that wraps an identifiable instance to provide state tracking
    /// </summary>
    /// <typeparam name="T"></typeparam>
        public class TrackedIdentifiable<T> : TrackedIdentifiable, ITrackedIdentifiable<T>
    {
        public TrackedIdentifiable(T instance, IRepository repository, ICopyIdentifiables copier, TrackingState state = TrackingState.Unknown)
            : base(instance as IIdentifiable, repository, copier, state)
        {}

        public new T Identifiable
        {
            get { return (T)base.Identifiable; }
            set { base.Identifiable = (IIdentifiable)value; }
        }

        public new T Original
        {
            get { return (T)base.Original; }
            set { base.Original = (IIdentifiable)value; }
        }
    }

    public abstract class TrackedIdentifiable : IDisposable, ITrackedIdentifiable
    {
        List<INotifyCollectionChanged> _observedCollections = new List<INotifyCollectionChanged>();
        protected TrackedIdentifiable(IIdentifiable instance, IRepository repository, ICopyIdentifiables copier, TrackingState state = TrackingState.Unknown)
        {
            this.IsObserving = false;
            this.Repository = repository;
            this.Copier = copier;
            this.ResetState(instance);

            if (Identifiable is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)Identifiable).PropertyChanged += TrackedIdentifiable_PropertyChanged;
                this.IsObserving = true;
            }

            if (Repository != null && Repository.Policy.LookForINotifyCollectionChanged)
            {
                var reflectingType = Identifiable.GetType();

                while (reflectingType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable)))
                {
                    foreach (var pi in reflectingType.GetTypeInfo().DeclaredProperties)
                    {
                        if (pi.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(INotifyCollectionChanged)))
                        {
                            var collection = pi.GetValue(instance) as INotifyCollectionChanged;
                            if (!_observedCollections.Contains(collection))
                            {
                                collection.CollectionChanged += Collection_CollectionChanged;
                                _observedCollections.Add(collection);
                            }
                        }
                    }
                    reflectingType = reflectingType.GetTypeInfo().BaseType;
                }
            }

            if (!this.IsObserving || state != TrackingState.Unknown)
            {
                this.State = state;
            }
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.State = TrackingState.ShouldSave;
        }

        private void TrackedIdentifiable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Id") 
                && !string.IsNullOrEmpty(Original.Id)
                && !string.IsNullOrEmpty(Identifiable.Id)
                && Original.Id != Identifiable.Id)
            {
                throw new InvalidOperationException("An Id value cannot be changed from a non-empty value to another non-empty value on a tracked instance.  Either load a new instance from the repository, or detach the instance prior to changing the Id.");
            }
            this.State = TrackingState.ShouldSave;
        }

        private TrackingState _state;
        public TrackingState State
        {
            get { return _state; }
            set { _state = value; }
        }

        private IIdentifiable Clone(IIdentifiable identifiable)
        {
            var isProxy = identifiable is IProxyIdentifiable;
            var type = identifiable.GetType();
            var canDeferLoad = false;
            IIdentifiable clone = null;
            if (isProxy)
            {
                canDeferLoad = ((IProxyIdentifiable)identifiable).CanDeferLoad;
                ((IProxyIdentifiable)identifiable).CanDeferLoad = false;
                ((IProxyIdentifiable)identifiable).IsInitializing = true;
            }
            try
            {
                clone = this.Copier.Clone(identifiable);
                return clone;
            }
            finally
            {
                if (isProxy)
                {
                    ((IProxyIdentifiable)identifiable).CanDeferLoad = canDeferLoad;
                    ((IProxyIdentifiable)identifiable).IsInitializing = false;
                    if (clone is IProxyIdentifiable)
                    {
                        ((IProxyIdentifiable)clone).IsInitializing = false;
                        ((IProxyIdentifiable)clone).CanDeferLoad = canDeferLoad;
                    }
                }
            }
            
        }

        private void Copy(IIdentifiable source, IIdentifiable dest)
        {
            var type = source.GetType();
            var canDeferLoad = false;
            var isProxy = source is IProxyIdentifiable;
            if (isProxy)
            {
                canDeferLoad = ((IProxyIdentifiable)source).CanDeferLoad;
                ((IProxyIdentifiable)source).CanDeferLoad = false;
                ((IProxyIdentifiable)source).IsInitializing = true;
                if (Identifiable is IProxyIdentifiable)
                {
                    ((IProxyIdentifiable)dest).IsInitializing = true;
                    ((IProxyIdentifiable)dest).CanDeferLoad = false;
                }
            }

            try
            {
                this.Copier.Copy(source, dest);
            }
            finally
            {
                if (isProxy)
                {
                    ((IProxyIdentifiable)source).CanDeferLoad = canDeferLoad;
                    ((IProxyIdentifiable)source).IsInitializing = false;
                    if (dest is IProxyIdentifiable)
                    {
                        ((IProxyIdentifiable)dest).CanDeferLoad = canDeferLoad;
                        ((IProxyIdentifiable)dest).IsInitializing = false;
                    }
                }
            }
        }

        /// <summary>
        /// The current Identifiable being watched.
        /// </summary>
        public IIdentifiable Identifiable { get; set; }

        /// <summary>
        /// If IsObservable is false, then this value will be non-null, and equivalent to a clone of the Identifiable prior to any changes.
        /// </summary>
        protected IIdentifiable Original { get; set; }

        /// <summary>
        /// Gets a value indicating whether modification state changes are being determined by observing INotifyPropertyChanged events (true), 
        /// or whether modification state will be determined by comparing cloned instances of the Identifiable between successive updates.
        /// </summary>
        public bool IsObserving { get; private set; }
        public IRepository Repository { get; private set; }
        public ICopyIdentifiables Copier { get; private set; }

        /// <summary>
        /// Determines the TrackingState for items in an Unknown state.  By supplying true for the optional 
        /// foreEvaluation parameter, all items other than those in a ShouldDelete will be evaluated.  If the tracked 
        /// instance implements INotifyPropertyChanged, then this method will simply return the current State value, without 
        /// performing any evaluation.
        /// </summary>
        /// <param name="forceReevaluation"></param>
        /// <returns></returns>
        public TrackingState CalculateState(bool forceEvaluation = false)
        {
            if (State == TrackingState.Unknown || forceEvaluation)
            {
                if ((forceEvaluation && State != TrackingState.ShouldDelete)
                    || State == TrackingState.Unknown)
                {
                    if (!string.IsNullOrEmpty(Original.Id) && Original.Id != Identifiable.Id)
                    {
                        throw new InvalidOperationException("An Id value cannot be changed from a non-empty value to another non-empty value on a tracked instance.  Either load a new instance from the repository, or detach the instance prior to changing the Id.");
                    }

                    if (State == TrackingState.ShouldSave && string.IsNullOrEmpty(Identifiable.Id)) return State;
                    if ((State == TrackingState.Unknown || State == TrackingState.IsUnchanged) && string.IsNullOrEmpty(Identifiable.Id))
                    {
                        State = TrackingState.ShouldSave;
                        return State;
                    }

                    State = TrackingState.IsUnchanged;

                    var type = Original.GetType();
                    var destType = Identifiable.GetType();
                    if (type != destType && type.GetTypeInfo().IsSubclassOf(destType))
                    {
                        type = destType; // get common base type to compare
                    }

                    var reflectingType = type;

                    while (reflectingType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable)))
                    {
                        foreach (var pi in reflectingType.GetTypeInfo().DeclaredProperties.Where(p => p.CanRead && p.CanWrite))
                        {
                            var piTypeInfo = pi.PropertyType.GetTypeInfo();
                            if (pi.PropertyType.IsArray
                                && piTypeInfo.GetElementType().GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable)))
                            {
                                var oE = pi.GetValue(Original) as IEnumerable;
                                var iE = pi.GetValue(Identifiable) as IEnumerable;
                                var oEn = oE == null ? null : oE.GetEnumerator();
                                var iEn = iE == null ? null : iE.GetEnumerator();
                                State = CompareEnumerableIIdentifiable(oEn, iEn);
                                if (State == TrackingState.ShouldSave)
                                    return State;
                            }
                            else if (piTypeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable))
                                && piTypeInfo.IsGenericType
                                && (piTypeInfo.GenericTypeArguments[0] == typeof(IIdentifiable)
                                    || piTypeInfo.GenericTypeArguments[0].GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable))))
                            {
                                var oE = pi.GetValue(Original) as IEnumerable;
                                var iE = pi.GetValue(Identifiable) as IEnumerable;
                                var oEn = oE == null ? null : oE.GetEnumerator();
                                var iEn = iE == null ? null : iE.GetEnumerator();
                                State = CompareEnumerableIIdentifiable(oEn, iEn);
                                if (State == TrackingState.ShouldSave)
                                    return State;
                            }
                            else
                            {
                                var oValue = GetValue(type, pi, Original);
                                var iValue = GetValue(type, pi, Identifiable);
                                if (oValue == null && iValue == null) continue;

                                if (oValue is IIdentifiable || iValue is IIdentifiable) // this case should never occur....
                                {
                                    if (oValue == null 
                                        || iValue == null 
                                        || ((IIdentifiable)oValue).Id != ((IIdentifiable)iValue).Id)
                                    {
                                        State = TrackingState.ShouldSave;
                                        return State;
                                    }
                                }
                                else if (oValue is IEnumerable || iValue is IEnumerable)
                                {
                                    if (!EnumerablesAreEquivalent(oValue as IEnumerable, iValue as IEnumerable))
                                    {
                                        State = TrackingState.ShouldSave;
                                        return State;
                                    }
                                }
                                else if (!ValueComparer.AreEqual(oValue, iValue))
                                {
                                    State = TrackingState.ShouldSave;
                                    return State;
                                }
                            }
                        }
                        reflectingType = reflectingType.GetTypeInfo().BaseType; // go down a level
                    }
                }
            }
            return State;
        }

        private bool EnumerablesAreEquivalent(IEnumerable a, IEnumerable b)
        {
            var aList = ToList(a);
            var bList = ToList(b);

            if (a == null || b == null || aList.Count != bList.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < aList.Count; i++)
                {
                    var value = aList[i];
                    if (!bList.Contains(value, new ValueComparer()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private IList<object> ToList(IEnumerable enumerable)
        {
            var list = new List<object>();
            if (enumerable != null)
            {
                var en = enumerable.GetEnumerator();
                while (en.MoveNext())
                {
                    list.Add(en.Current);
                }
            }
            return list;
        }

        private object GetValue(Type type, PropertyInfo pi, IIdentifiable instance)
        {
            if (pi.PropertyType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiable))
                && type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IProxyIdentifiable)))
            {
                // don't engage a defer load on a proxied type - check if it's already loaded first
                var isLoadedField = type.GetTypeInfo().DeclaredFields.FirstOrDefault(p => p.Name.Equals("_" + pi.Name + "IsLoaded"));
                if (isLoadedField == null) return null; // not loadable, return null
                var isLoaded = (bool)isLoadedField.GetValue(instance);
                if (isLoaded)
                {
                    // just get the pre-loaded value
                    var value = (IIdentifiable)pi.GetValue(instance);
                    return value == null ? null : value.Id; // we only care if the Id changed
                }
                else
                {
                    // not loaded, check for unloaded Id value
                    var idField = type.GetTypeInfo().DeclaredFields.Single(p => p.Name.Equals("____" + pi.Name + "Id"));
                    var id = (string)idField.GetValue(instance);
                    return id; // will either be an actual Id or Null
                }
            }
            else
            {
                return pi.GetValue(instance);
            }
        }


        /// <summary>
        /// Sets the state to Unknown, and resets the internal tracking reference to the current Identifiable state.
        /// </summary>
        public void ResetState()
        {
            Original = Clone(Identifiable);
            if (string.IsNullOrEmpty(Identifiable.Id))
            {
                State = TrackingState.ShouldSave;
            }
            else
            {
                State = TrackingState.Unknown;
            }
        }

        /// <summary>
        /// Sets the state to Unknown, and resets the internal tracking reference to the current Identifiable state.
        /// </summary>
        public void ResetState(IIdentifiable instance)
        {
            if (Identifiable == null)
                Identifiable = instance;
            else
                Copy(instance, Identifiable); // copy new values into old reference

            Original = Clone(Identifiable);

            if (string.IsNullOrEmpty(Identifiable.Id))
            {
                State = TrackingState.ShouldSave;
            }
            else
            {
                State = TrackingState.Unknown;
            }
        }

        private TrackingState CompareEnumerableIIdentifiable(IEnumerator originalEn, IEnumerator currentEn)
        {
            if (originalEn == null && currentEn == null) return TrackingState.IsUnchanged;
            if (originalEn == null || currentEn == null) return TrackingState.ShouldSave;

            var oIds = new HashSet<string>();
            var iIds = new HashSet<string>();

            while (originalEn.MoveNext())
            {
                var value = (IIdentifiable)originalEn.Current;
                oIds.Add(value == null ? null : value.Id);
            }

            while (currentEn.MoveNext())
            {
                var value = (IIdentifiable)currentEn.Current;
                iIds.Add(value == null ? null : value.Id);
            }

            if (oIds.Count != iIds.Count)
            {
                return TrackingState.ShouldSave;
            }

            foreach (var id in oIds)
            {
                if (!iIds.Contains(id))
                {
                    return TrackingState.ShouldSave;
                }
            }

            return TrackingState.IsUnchanged;
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (!_disposed)
            {
                if (this.Identifiable != null)
                {
                    if (this.Identifiable is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)this.Identifiable).PropertyChanged -= TrackedIdentifiable_PropertyChanged;
                    }
                    this.Identifiable = null;

                    foreach (var collection in _observedCollections)
                    {
                        collection.CollectionChanged -= Collection_CollectionChanged;
                    }
                    _observedCollections.Clear();
                }
                _disposed = true;
            }
        }

        class ValueComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                if (x == null && y == null) return true;
                return y != null && x != null && x.Equals(y);
            }

            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }

            public static bool AreEqual(object x, object y)
            {
                return new ValueComparer().Equals(x, y);
            }
        }
    }
}
