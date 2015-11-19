using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IActivitySet<T> : IActivitySet, IIdentifiableSet<T>
        where T : IActivity
    {
    }

    public interface IActivitySet : IIdentifiableSet
    {
    }
}
