using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SHS.Sage.Linq;

namespace SHS.Sage.Schema
{
    public class OClass<TRepository> : IClass<TRepository>
        where TRepository : ORepository
    {
        public const string V = "V";
        public const string E = "E";

        List<OProperty> _properties = new List<OProperty>();
        public OClass(string baseClass)
        {
            BaseClass = baseClass;
        }

        public string BaseClass { get; private set; }

        public ClassType ClassType { get; set; }

        public string IdMask
        {
            get
            {
                return @"#(?<cluster>\d+):(?<id>\d+)";
            }
        }

        public bool IsAbstract { get; set; }

        public string Name { get; set; }

        public IEnumerable<IProperty> Properties 
        { 
            get
            {
                return _properties.ToReadOnly();
            }
        }

        public string[] Clusters { get; set; }

        public string DefaultCluster { get; set; }

        public bool IsMaskedId(string id)
        {
            var regex = new Regex(IdMask);
            var match = regex.Match(id);
            var isMatch = false;
            if (match.Success)
            {
                var cluster = match.Groups["cluster"].Value;
                isMatch = Clusters.Contains(cluster);
            }
            return isMatch;
        }

        public void AddProperty(IProperty property)
        {
            _properties.Add((OProperty)property);
        }

        public void RemoveProperty(IProperty property)
        {
            _properties.Remove((OProperty)property);
        }

        public void AlterProperty(IProperty property)
        {
            var prop = _properties.SingleOrDefault(p => p.Name == property.Name);
            if (prop == null)
            {
                AddProperty(property);
            }
            else
            {
                prop.IsNotNull = property.IsNotNull;
                prop.IsReadOnly = property.IsReadOnly;
                prop.IsRequired = property.IsRequired;
                prop.PropertyType = property.PropertyType;
            }
        }
    }
}
