using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Schema
{
    public class OProperty : IProperty
    {
        public bool IsNotNull { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsRequired { get; set; }

        public string Name { get; set; }

        public int PropertyType { get; set; }

        public string PropertyTypeText()
        {
            return ((OPropertyType)PropertyType).ToString();
        }
    }
}
