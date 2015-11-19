using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.UnitOfWork
{
    public class TrackingEnumerator<T> : IEnumerable<T>
    {
        private IEnumerable<T> _enumerable;
        private ITrackingRepository _repository;

        public TrackingEnumerator(IEnumerable<T> enumerable, ITrackingRepository repository)
        {
            this._enumerable = enumerable;
            this._repository = repository;
        }


        public IEnumerable<T> Source { get { return _enumerable; } }
        public ITrackingRepository Repository { get { return _repository; } }

        public IEnumerator<T> GetEnumerator()
        {
            foreach(var item in Source)
            {
                if (this.Repository.Policy.TrackChanges)
                {
                    var state = this.Repository.GetState<T>(item);
                    if (this.Repository.Policy.ReturnTrackedDeletes
                        || state != TrackingState.ShouldDelete) // suppress items marked for delete
                    {
                        yield return item;
                    }
                }
                else yield return item; // just return it, we're not tracking
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
