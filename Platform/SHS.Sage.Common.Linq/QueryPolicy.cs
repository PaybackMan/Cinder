using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    /// <summary>
    /// Defines query execution & materialization policies. 
    /// </summary>
    public class QueryPolicy : IQueryPolicy
    {
        /// <summary>
        /// Gets/sets the maximum number of primary results to return per query.  Default (-1) is unlimited.
        /// </summary>
        public virtual int Limit { get; set; }
        /// <summary>
        /// Gets/sets boolean to enable/disable proxy generation to allow for lazy-loaded deep graph traversal of related types.  
        /// Default (true) enables lazy loading.
        /// </summary>
        public virtual bool DeferLoadComplexTypes { get; set; }
        /// <summary>
        /// Gets/sets boolean to enable/disable the use of the compiled query cache
        /// </summary>
        public virtual bool UseQueryCache { get; set; }
        /// <summary>
        /// Gets/sets boolean to enable/disable entity change tracking for queried entity instances
        /// </summary>
        public virtual bool TrackChanges { get; set; }

        public bool ReturnTrackedDeletes { get; set; }

        /// <summary>
        /// Gets/sets a boolean to enable/disable examination of tracked entity properties which implement INotifyCollectionChanged, allowing the Tracking State Manager 
        /// to update tracked item states to ShouldSave when collection properties are changed.  If your entity type internally detects INotifyCollectionChanged events, 
        /// and also implements INotifyPropertyChanged and raises the PropertyChanged event, then you should set this value to false, as there is a performance penalty 
        /// imposed to scan the type for INotifyCollectionChanged properties when adding to the tracking system.
        /// </summary>
        public bool LookForINotifyCollectionChanged { get; set; }

        public QueryPolicy()
        {
        }

        /// <summary>
        /// Determines if a relationship property is to be included in the results of the query
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsIncluded(MemberInfo member)
        {
            var type = TypeHelper.GetMemberType(member);
            return type.Implements(typeof(IIdentifiable)) 
                || (type.IsGenericType && type.Implements(typeof(IEnumerable<>)) && type.GetGenericArguments()[0].Implements(typeof(IIdentifiable)));
        }

        /// <summary>
        /// Determines if a relationship property is included, but the query for the related data is 
        /// deferred until the property is first accessed.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsDeferLoaded(MemberInfo member)
        {
            return DeferLoadComplexTypes && false;
        }

        public virtual QueryPolice CreatePolice(QueryTranslator translator)
        {
            return new QueryPolice(this, translator);
        }

        public bool Equals(IQueryPolicy other)
        {
            return other != null
                && this.Limit == other.Limit
                && this.DeferLoadComplexTypes == other.DeferLoadComplexTypes;
        }
    }
}
