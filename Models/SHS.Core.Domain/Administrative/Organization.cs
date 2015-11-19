using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cinder.Core.Domain.Common;
using Cinder.Core.Domain.Person;


namespace Cinder.Core.Domain.Administrative
{
///================================================================================================================================
/// <summary>
/// A formally or informally recognized grouping of people or organizations formed for the purpose of achieving some form of collective
/// action. Includes companies, institutions, corporations, departments, community groups, healthcare practice groups, etc. 
/// 
/// Organization is used for collections of people that have come together to achieve an objective. The Group resource is used to 
/// identify a collection of people (or animals, devices, etc.) that are gathered for the purpose of analysis or acting upon, but are 
/// not expected to act themselves
/// 
/// </summary>
///===============================================================================================================================
    public class Organization : INotifyPropertyChanged
    {
        private string _name;
        private CodeableConcept _type;
        private ContactMethod _telecom;
        private Organization _partOf;
        private bool _isActive;
        private ObservableCollection<Address> _addresses          = new ObservableCollection<Address>();
        private ObservableCollection<Location> _locations         = new ObservableCollection<Location>();
        private ObservableCollection<Contact> _contacts           = new ObservableCollection<Contact>();
        private ObservableCollection<Organization> _organizations = new ObservableCollection<Organization>();
        private ObservableCollection<Person.Person> _people       = new ObservableCollection<Person.Person>();
        private byte[] _image; 

///===============================================================================================================================
/// <summary>
/// Default Ctor.. 
/// </summary>
///===============================================================================================================================
        public Organization()
        { }

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
                OnPropertyChanged(nameof(Id));
            }
        }
///===============================================================================================================================
/// <summary>
/// Decendant Organizations. These are either departments or some affiliated external Organization like a Distributor
/// </summary>
///===============================================================================================================================
        public virtual ObservableCollection<Organization> Organizations
        {
            get { return _organizations; }
            set { _organizations = value; }
        }
///===============================================================================================================================
/// <summary>
/// A pic of the ORganization
/// </summary>
///===============================================================================================================================
        public byte[] Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
///===============================================================================================================================
/// <summary>
/// The organization of which this organization forms a part.
/// </summary>
///===============================================================================================================================
        public virtual Organization Parent
        {
            get { return _partOf; }
            set
            {
                _partOf = value;
                OnPropertyChanged(nameof(Parent));
            }
        }
///===============================================================================================================================
/// <summary>
///Whether the organization's record is still in active use.
/// </summary>
///===============================================================================================================================
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
///===============================================================================================================================
/// <summary>
/// Name used for the organization 
/// </summary>
///===============================================================================================================================
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
///===============================================================================================================================
/// <summary>
/// Kind of organization 
/// </summary>
///===============================================================================================================================
        public virtual CodeableConcept Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }
///===============================================================================================================================
/// <summary>
/// A contact detail for the organization  
/// </summary>
///===============================================================================================================================
        public virtual ContactMethod ContactMethod
        {
            get { return _telecom; }
            set
            {
                _telecom = value;
                OnPropertyChanged(nameof(ContactMethod));
            }
        }
///===============================================================================================================================
/// <summary>
/// A contact detail for the organization  
/// </summary>
///===============================================================================================================================
        public virtual ObservableCollection<Address> Addresses
        {
            get { return _addresses; }
            set { _addresses = value; }
        }
///===============================================================================================================================
/// <summary>
///  Location(s) the organization uses to provide services  
/// </summary>
///===============================================================================================================================
        public virtual ObservableCollection<Location> Locations
        {
            get { return _locations; }
            set { _locations = value; }
        }
///===============================================================================================================================
/// <summary>
/// Contact for the organization for a certain purpose 
/// </summary>
///===============================================================================================================================
        public virtual ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }
///===============================================================================================================================
/// <summary>
/// Practitioners, Contacts or Patients associated with the ORganization.
/// </summary>
///===============================================================================================================================
        public virtual ObservableCollection<Person.Person> People
        {
            get { return _people; }
            set { _people = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
