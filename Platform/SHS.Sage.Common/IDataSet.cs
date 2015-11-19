using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IDataSet<T> : IDataSet, IOrderedQueryable<T>
    { }

    public interface IDataSet : IOrderedQueryable
    {
        /// <summary>
        /// Gets the IRepository instance associated with this IThingSet
        /// </summary>
        IRepository Repository { get; }
    }
}
