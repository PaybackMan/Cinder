using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Core.Domain.Administrative
{
    public enum ContactSystem
    {
        /// <summary> The value is a telephone number used for voice calls. Use of full international numbers starting with + is recommended to enable automatic dialing support but not required. </summary>
        Phone,
        /// <summary> The value is a fax machine. Use of full international numbers starting with + is recommended to enable automatic dialing support but not required. </summary>    
        Fax,
        /// <summary> The value is an email address </summary>
        Email,
        /// <summary> The value is a url. This is intended for various personal contacts including blogs, Twitter, Facebook, etc. Do not use for email addresses. </summary>
        Url
    }
    public enum ContactUse
    {
        /// <summary> A communication contact at a home; attempted contacts for business purposes might intrude privacy and chances are one will contact family or other household members instead of the person one wishes to call. Typically used with urgent cases, or if no other contacts are available.</summary> 
        Home,
        /// <summary> An office contact. First choice for business related contacts during business hours.</summary>
        Work,
        /// <summary> A temporary contact. The period can provide more detailed information.</summary>
        Temp,
        /// <summary> This contact is no longer in use (or was never correct, but retained for records).</summary>
        Old,
        /// <summary> A telecommunication device that moves and stays with its owner. May have characteristics of all other use codes, suitable for urgent matters, not the first choice for routine business.</summary>
        Mobile
    }

///===============================================================================================================================
/// <summary>
/// All kinds of technology-mediated contact details for a person or organization, including telephone, email, etc. 
/// </summary>
///===============================================================================================================================
    public class ContactMethod 
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
        private ContactSystem _system;
        private ContactUse _use;
        private string _value;
        private Period _period;

///===============================================================================================================================
/// <summary>
/// Default Ctor.. 
/// </summary>
///===============================================================================================================================
        public ContactMethod()
        {}
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="id"></param>
/// <param name="_system"></param>
/// <param name="_use"></param>
/// <param name="_value"></param>
///===============================================================================================================================
        public ContactMethod(ContactSystem _system, ContactUse _use, string _value)
        {
            this._system = _system;
            this._use    = _use;
            this._value  = _value;
        }
///===============================================================================================================================
/// <summary>
/// Time period when the contact was/is in use 
/// </summary>
///===============================================================================================================================
        public Period Period
        {
            get { return _period; }
            set { _period = value; }
        }
///===============================================================================================================================
/// <summary>
/// The actual contact details
/// </summary>
///===============================================================================================================================
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
///===============================================================================================================================
/// <summary>
/// Telecommunications form for contact
/// </summary>
///===============================================================================================================================
        public ContactSystem System
        {
            get { return _system; }
            set { _system = value; }
        }
///===============================================================================================================================
/// <summary>
/// Location, type or status of telecommunications address indicating use
/// </summary>
///===============================================================================================================================
        public ContactUse Use
        {
            get { return _use; }
            set { _use = value; }
        }
    }
}
