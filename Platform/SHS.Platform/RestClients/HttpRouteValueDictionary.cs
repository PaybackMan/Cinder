using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace Cinder.Platform.RestClients
{
    public class HttpRouteValueDictionary : Dictionary<string, object>
    {
        public HttpRouteValueDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public HttpRouteValueDictionary(IDictionary<string, object> dictionary)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            if (dictionary != null)
            {
                foreach (KeyValuePair<string, object> current in dictionary)
                {
                    Add(current.Key, current.Value);
                }
            }
        }

        public HttpRouteValueDictionary(object values)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            IDictionary<string, object> valuesAsDictionary = values as IDictionary<string, object>;
            if (valuesAsDictionary != null)
            {
                foreach (KeyValuePair<string, object> current in valuesAsDictionary)
                {
                    Add(current.Key, current.Value);
                }
            }
            //else if (values != null)
            //{
            //    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(values);
            //    foreach (PropertyDescriptor prop in properties)
            //    {
            //        object val = prop.GetValue(values);
            //        Add(prop.Name, val);
            //    }
            //}
        }
    }

}
