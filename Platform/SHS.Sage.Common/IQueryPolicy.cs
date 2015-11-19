using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IQueryPolicy : IEquatable<IQueryPolicy>
    {
        /// <summary>
        /// Gets/sets the maximum number of primary results to return per query.  Default (-1) is unlimited.
        /// </summary>
        int Limit { get; set; }
        /// <summary>
        /// Gets/sets boolean to enable/disable proxy generation to allow for lazy-loaded deep graph traversal of related types.  
        /// Default (true) enables lazy loading.
        /// </summary>
        bool DeferLoadComplexTypes { get; set; }
        /// <summary>
        /// Gets/sets boolean to enable/disable the use of the compiled query cache
        /// </summary>
        bool UseQueryCache { get; set; }
        /// <summary>
        /// Get/sets boolean to enable/disable change tracking of entity instances returned from read queries (Get, Queryable, ExecuteEnumerable).
        /// </summary>
        bool TrackChanges { get; set; }
        /// <summary>
        /// Gets/sets a boolean to enable/disable returning tracked items with unsaved Delete status when reading from the repository.  If set to false, 
        /// Enumerable lists will omit the Delete items from their results, and Get calls will return NULL instances.
        /// </summary>
        bool ReturnTrackedDeletes { get; set; }
        /// <summary>
        /// Gets/sets a boolean to enable/disable examination of tracked entity properties which implement INotifyCollectionChanged, allowing the Tracking State Manager 
        /// to update tracked item states to ShouldSave when collection properties are changed.  If your entity type internally detects INotifyCollectionChanged events, 
        /// and also implements INotifyPropertyChanged and raises the PropertyChanged event, then you should set this value to false, as there is a performance penalty 
        /// imposed to scan the type for INotifyCollectionChanged properties when adding to the tracking system.
        /// </summary>
        bool LookForINotifyCollectionChanged { get; set; }
    }
}
