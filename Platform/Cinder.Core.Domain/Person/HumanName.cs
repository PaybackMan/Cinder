using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;

namespace Cinder.Core.Domain.Person
{
///===============================================================================================================================
/// <summary>
/// A name of a human with text, parts and usage information. 
/// Names may be changed or repudiated. People may have different names in different contexts. Names may be divided into parts of different type
///  that have variable significance depending on context, though the division into parts is not always significant. With personal names, the different 
/// parts may or may not be imbued with some implicit meaning; various cultures associate different importance with the name parts and the degree to which
///  systems SHALL care about name parts around the world varies widely.
/// </summary>
///===============================================================================================================================
    public class HumanName
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
        private NameUse _use = NameUse.Usual;
        private string _text;
        private string _family;
        private string _given;
        private string _prefix;
        private string _suffix;
        private Period _period;

///===============================================================================================================================
/// <summary>
/// Default Ctor.. 
/// </summary>
///===============================================================================================================================
        public HumanName() { }
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="given"></param>
/// <param name="family"></param>
///===============================================================================================================================
        public HumanName(string given, string family)
    {
        _given  = given;
        _family = family;
    }
///===============================================================================================================================
/// <summary>
///   Time period when name was/is in use 
/// </summary>
///===============================================================================================================================
        public Period Period
        {
            get
            {
                return _period;
            }

            set
            {
                _period = value;
            }
        }
///===============================================================================================================================
/// <summary>
///  Parts that come after the name 
/// </summary>
///===============================================================================================================================
        public string Suffix
        {
            get
            {
                return _suffix;
            }

            set
            {
                _suffix = value;
            }
        }
///===============================================================================================================================
/// <summary>
///Parts that come before the name 
/// </summary>
///===============================================================================================================================
        public string Prefix
        {
            get
            {
                return _prefix;
            }

            set
            {
                _prefix = value;
            }
        }
///===============================================================================================================================
/// <summary>
/// Given names (not always 'first'). Includes middle names 
/// </summary>
///===============================================================================================================================
        public string Given
        {
            get
            {
                return _given;
            }

            set
            {
                _given = value;
            }
        }
///===============================================================================================================================
/// <summary>
/// Family name (often called 'Surname')  
/// </summary>
///===============================================================================================================================
        public string Family
        {
            get
            {
                return _family;
            }

            set
            {
                _family = value;
            }
        }
///===============================================================================================================================
/// <summary>
/// Text representation of the full name 
/// </summary>
///===============================================================================================================================
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
            }
        }
///===============================================================================================================================
/// <summary>
/// This value set defines its own terms in the system http://hl7.org/fhir/name-use
/// </summary>
///===============================================================================================================================
        public NameUse Use
        {
            get
            {
                return _use;
            }

            set
            {
                _use = value;
            }
        }

    }
}
