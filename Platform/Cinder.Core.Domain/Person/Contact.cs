using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;
using Cinder.Core.Domain.Common;

namespace Cinder.Core.Domain.Person
{
    public class Contact  : Person
    {
       public Contact(HumanName humanName, ObservableCollection<ContactMethod> contactMethods, DateTime birthDate, Address address) : base (humanName, contactMethods, birthDate, address)
       {}

        public Contact()
        {
            
        }
    }
}
