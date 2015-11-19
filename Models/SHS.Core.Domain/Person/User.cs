using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;

namespace Cinder.Core.Domain.Person
{
    public class User : IIdentity
    {
        private bool _isAuthenticated = false;
        public User()
        { }

        public User(bool isAuthenticated)
        {
            _isAuthenticated = isAuthenticated;
        }
        public string Email { get; set; }
        public string Password { get; set; }
        private string _id;
        ///===============================================================================================================================
        /// <summary>
        /// Unique identifier of the Entity.. 
        /// </summary>
        ///===============================================================================================================================
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                //OnPropertyChanged(nameof(Id));
            }
        }
        public string AuthenticationType { get; }
        public bool IsAuthenticated { get { return _isAuthenticated; } }
        public string Name { get; set; }
        public Organization Organization { get; set; }
        public virtual ObservableCollection<Claim> Claims { get; set; }
        /// <summary>
        /// The participating Actor that has User security privileges within the system
        /// </summary>
       

    }
}
