using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IThingSet<T> : IIdentifiableSet<T>, IThingSet
        where T : IThing
    {
        /// <summary>
        /// Returns a new AssociationSet of type U, sourced from sourceThing of type T
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="sourceThing"></param>
        /// <returns></returns>
        IAssociationSet<U> AssociationSet<U>(T sourceThing) where U : IAssociation;
    }

    public interface IThingSet : IIdentifiableSet
    {
    }
}
