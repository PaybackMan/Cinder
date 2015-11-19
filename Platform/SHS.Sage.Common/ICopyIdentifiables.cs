using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface ICopyIdentifiables
    {
        IIdentifiable Clone(IIdentifiable source);
        void Copy(IIdentifiable source, IIdentifiable destination);
    }
}
