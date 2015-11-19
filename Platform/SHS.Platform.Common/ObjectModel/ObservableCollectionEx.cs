using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Platform.ObjectModel
{
    public static class ObservableCollectionEx
    {
        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> list)
        {
            var oc = new ObservableCollection<T>(list);
            return oc;
        }
    }
}
