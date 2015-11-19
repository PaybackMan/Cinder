using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;
using Cinder.Core.Domain.Common;

namespace Cinder.Core.Domain.Person
{
    public class Person 
    {
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
        private HumanName _humanName;
        private ObservableCollection<ContactMethod> _contactMethods = new ObservableCollection<ContactMethod>();
        private DateTime _birthDate;
        private byte[] _photo;
        private Address _address;

///===============================================================================================================================
/// <summary>
/// Default Ctor
/// </summary>
///===============================================================================================================================
        public Person(){}
///===============================================================================================================================
/// <summary>
/// Fat daddy ctor
/// </summary>
/// <param name="id"></param>
/// <param name="humanName"></param>
/// <param name="contactMethods"></param>
/// <param name="birthDate"></param>
/// <param name="photo"></param>
/// <param name="address"></param>
// ===============================================================================================================================
        public Person (HumanName humanName, ObservableCollection<ContactMethod> contactMethods, DateTime birthDate, Address address)
        {
            this.Name           = humanName;
            this.ContactMethods = contactMethods;
            this.BirthDate      = birthDate;
            this._address       = address;
        }
///===============================================================================================================================
/// <summary>
/// A name of a human with text, parts and usage information.
/// </summary>
///===============================================================================================================================
        public HumanName Name
        {
            get { return _humanName; }
            set { _humanName = value; }
        }
///===============================================================================================================================
/// <summary>
/// A contact detail for the practitioner, e.g. a telephone number or an email address. 
/// </summary>
///===============================================================================================================================
        public ObservableCollection<ContactMethod> ContactMethods
        {
            get { return _contactMethods; }
            set { _contactMethods = value; }
        }
///===============================================================================================================================
/// <summary>
/// The date and time of birth for the Person.
/// </summary>
///===============================================================================================================================
        public DateTime BirthDate
        {
            get { return _birthDate; }
            set { _birthDate = value; }
        }
///===============================================================================================================================
/// <summary>
/// Image of the person. 
/// </summary>
///===============================================================================================================================
        public byte[] Photo
        {
            get { return _photo; }
            set { _photo = value; }
        }
///===============================================================================================================================
/// <summary>
/// The postal address where the Person can be found or visited or to which mail can be delivered.
/// </summary>
///===============================================================================================================================
        public Address Address 
        {
            get { return _address; }
            set { _address = value; }
        }
    }
}
