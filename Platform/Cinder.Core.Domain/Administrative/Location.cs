using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Core.Domain.Common;

namespace Cinder.Core.Domain.Administrative
{
///================================================================================================================================
/// <summary>
/// Details and position information for a physical place where services are provided and resources and participants may be stored,
/// found, contained or accommodated.
/// 
/// 5.6.1 Scope and Usage
/// 
/// A Location includes both incidental locations(a place which is used for healthcare without prior designation or authorization) 
/// and dedicated, formally appointed locations.Locations may be private, public, mobile or fixed and scale from small freezers to 
/// full hospital buildings or parking garages.
/// 
/// Examples of Locations are:
///
///  - Building, ward, corridor or room
///  - Freezer, incubator
///  - Vehicle or lift
///  - Home, shed, or a garage
///  - Road, parking place, a park
/// 
/// </summary>
///================================================================================================================================
    public partial class Location 
    {
        private string _name;
        private string _description;
        private ObservableCollection<ContactMethod> _contactMethods;
        private ObservableCollection<Location> _locations;
        private LocationStatus _status;
        private Address _address;
        private Organization _organization;
        private Location _parent;
        private Position _position;

///===============================================================================================================================
/// <summary>
/// Default Ctor.. 
/// </summary>
///===============================================================================================================================
        public Location() { }
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
///===============================================================================================================================
/// <summary>
/// Name of the location as used by humans. Does not need to be unique.
/// </summary>
///===============================================================================================================================
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
///===============================================================================================================================
/// <summary>
///The contact details of communication devices available at the location. This can include phone numbers, fax numbers, mobile numbers, 
/// email addresses and web sites.
/// </summary>
///===============================================================================================================================
        public virtual ObservableCollection<ContactMethod> ContactMethods
        {
            get { return _contactMethods; }
            set { _contactMethods = value; }
        }
///===============================================================================================================================
/// <summary>
/// Description of the Location, which helps in finding or referencing the place.
/// </summary>
///===============================================================================================================================
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
///===============================================================================================================================
/// <summary>
/// Indicates whether the location is still in use
/// </summary>
///===============================================================================================================================
        public LocationStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }
///===============================================================================================================================
/// <summary>
/// Physical location.
/// </summary>
///===============================================================================================================================
        public Address Address 
        {
            get { return _address; }
            set { _address = value; }
        }
///===============================================================================================================================
/// <summary>
/// The organization that is responsible for the provisioning and upkeep of the location.
/// </summary>
///===============================================================================================================================
        public virtual Organization Organization
        {
            get { return _organization; }
            set { _organization = value; }
        }
///===============================================================================================================================
/// <summary>
/// Another Location which this Location is physically part of. For purposes of location, display and identification, knowing which
///  locations are located within other locations is important.
/// </summary>
///===============================================================================================================================
        public virtual Location Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
///===============================================================================================================================
/// <summary>
/// Sub location from the current one..
/// </summary>
///===============================================================================================================================
        public virtual ObservableCollection<Location> Locations
        {
            get { return _locations; }
            set { _locations = value; }
        }
///===============================================================================================================================
/// <summary>
///  The absolute geographic location of the Location, expressed in a KML compatible manner. For mobile applications and automated
///  route-finding knowing the exact location of the Location is required.
/// </summary>
///===============================================================================================================================
        public virtual Position Position
        {
            get { return _position; }
            set { _position = value; }
        }
    }
}
