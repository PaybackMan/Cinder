using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface ICopyable
    {
        void CopyTo(Object destination);
        void CopyFrom(Object source);
    }

    public interface ICopyable<T> : ICopyable
    {
        void CopyTo(T destination);
        void CopyFrom(T source);
    }
}
