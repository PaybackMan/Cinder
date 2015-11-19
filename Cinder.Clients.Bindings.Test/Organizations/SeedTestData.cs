using Cinder.Core.Domain.Administrative;
using Cinder.Core.Domain.Common;
using System.Collections.ObjectModel;

namespace Cinder.Clients.Bindings.Test.Organizations
{
    public static class SeedTestData
    {
//=================================================================================================================
/// <summary>
/// 
/// </summary>
/// <returns></returns>
//=================================================================================================================
        public static Organization CreateTestOrganization()
        {
            var contactMethod = new ContactMethod
            {
                System = ContactSystem.Phone,
                Value = "281-897-8766",
                Id = "553ft8246-D56B-419F-9FBC-DC1E5C920885"
            };

            var org = new Organization
            {
                Name = "Acme Inc",
                Id = "557D8246-D56B-419F-9FBC-DC1E5C920885",
                ContactMethod = contactMethod,
                Type = new CodeableConcept("TestOrg"),
                Addresses = SeedTestData.GetTestAddress(),

            };

            return org;
        }
//=================================================================================================================
/// <summary>
/// 
/// </summary>
/// <returns></returns>
//=================================================================================================================
        private static ObservableCollection<Location> GetTestLocations()
        {
            var results = new ObservableCollection<Location>();
            var location1 = new Location
            {
                Name = "Downtown Complex",
                Description = "Main Research Campus for Ac me Inc."
            };

            var location2 = new Location
            {
                Name = "Westside Kiosk",
                Description = "Small Shop for setting appointments"
            };

            var location3 = new Location
            {
                Name = "Headquaters",
                Description = "A sprawling campus hidden underground."
            };

            results.Add(location1);
            results.Add(location2);
            results.Add(location3);

            return results;
        }
//=================================================================================================================
/// <summary>
/// 
/// </summary>
/// <returns></returns>
//=================================================================================================================
        private static ObservableCollection<Address> GetTestAddress()
        {
            var address1 = new Address
            {
                City = "Baton Rouge",
                State = "LA",
                Line = "12747 LockHaven",
                PostalCode = "77543"
            };

            var address2 = new Address
            {
                City = "Baton Rouge",
                State = "LA",
                Line = "16739 Tiger Rd.",
                PostalCode = "77544"
            };

            var address3 = new Address
            {
                City = "Jenkins",
                State = "LA",
                Line = "16739 Crawdad Rd.",
                PostalCode = "77544"
            };

            var results = new ObservableCollection<Address>();

            results.Add(address1);
            results.Add(address2);
            results.Add(address3);

            return results;
        }
    }
}
