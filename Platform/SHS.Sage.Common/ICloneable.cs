using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface ICloneable<T> : ICloneable
    {
        new T Clone();
    }

    public interface ICloneable
    {
        object Clone();
    }
}
