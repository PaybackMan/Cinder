using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IScalar<T> where T : struct
    {
        T Value { get; set; }
    }
}
