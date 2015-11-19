using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IPermission : IIdentifiable
    {
        string Name { get; set; }
    }
}
