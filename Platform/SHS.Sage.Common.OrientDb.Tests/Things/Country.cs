using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.OrientDb.Tests.Things
{
    public class Country : IThing
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
