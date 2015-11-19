using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IIdentifiableSet<T> : IIdentifiableSet, IDataSet<T>
        where T : IIdentifiable
    {
        /// <summary>
        /// Gets a populated Thing instance for the provided activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        T Get(T identity);
    }

    public interface IIdentifiableSet : IDataSet
    {
        /// <summary>
        /// Gets a populated IIdentifiable instance with the specified IIdentity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        IIdentifiable Get(IIdentifiable identity);
    }
}
