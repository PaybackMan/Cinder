using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orient.Client;
using SHS.Core.Repository;
using SHS.Core.Domain.Administrative;
using SHS.Core.Domain.Common;
using SHS.Core.Domain.Person;

namespace SHS.Core.Domain.Tests
{
    public enum UserType
    {
        Admin,
        User
    }

    [TestClass]
    public class RepositoryTests
    {
        private const string CONNECTION_NAME = "TDB";
        private const int CREATE_USER_COUNT  = 4;
        static bool _isSetup = false;

///===============================================================================================================================
/// <summary>
/// 
/// </summary>
///===============================================================================================================================
        private void SeedOganizations()
        {
            string rootOrganizationName    = "ForeMedical";
            string distributorName         = "Spinal Elements";
            string surgeonOrganizationName = "Bobs Surgery Inc.";

            using (var repo = new ShsRepository(new ODatabase(CONNECTION_NAME)))
            {
                // Create Distributors.

                var spinalElements = repo.Create(() => new Organization
                {
                    Name          = distributorName,
                    IsActive      = true,
                    ContactMethod = repo.Create(() => new ContactMethod(ContactSystem.Email, ContactUse.Work, "HelpDesk@" + distributorName + ".com"))
                });

                spinalElements.Addresses.Add(this.CreateAddress(distributorName, repo));
                spinalElements.Contacts.Add(this.CreateOrganizationContact(distributorName, repo));
                spinalElements.People = this.CreatePeople(spinalElements, repo);

                // Create Surgeon Organizations..

                var surgeonOrganization = repo.Create(() => new Organization
                {
                    Name          = surgeonOrganizationName,
                    IsActive      = true,
                    ContactMethod = new ContactMethod(ContactSystem.Email, ContactUse.Work, "HelpDesk@" + surgeonOrganizationName + ".com")
                });

                surgeonOrganization.Addresses.Add(this.CreateAddress(surgeonOrganizationName, repo));
                surgeonOrganization.Contacts.Add(this.CreateOrganizationContact(surgeonOrganizationName, repo));
                surgeonOrganization.People = this.CreatePeople(surgeonOrganization, repo);
                
                // Create our main Org..

                var foreMedical = repo.Create<Organization>(() => new Organization
                {
                    Name = rootOrganizationName,
                    IsActive = true,
                    ContactMethod = new ContactMethod(ContactSystem.Email, ContactUse.Work, "HelpDesk@" + rootOrganizationName + ".com")
                });

                foreMedical.Addresses.Add(this.CreateAddress(rootOrganizationName, repo));
                foreMedical.Contacts.Add(this.CreateOrganizationContact(rootOrganizationName, repo));
                foreMedical.Organizations.Add(surgeonOrganization);
                foreMedical.Organizations.Add(spinalElements);
                foreMedical.People = this.CreatePeople(foreMedical, repo);

                //var org = repo.Create<Organization>(o => o.Name = "ForeMedical");
                //repo.Attach(foreMedical);
                repo.SaveChanges();
            }
        }
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="organizationName"></param>
/// <returns></returns>
///===============================================================================================================================
        private Contact CreateOrganizationContact(string organizationName, ShsRepository repo)
        {
            return repo.Create<Contact>(() => new Contact
            {
                Name      = new HumanName(organizationName, " Main Contact guy"),
                Address   = this.CreateAddress(organizationName, repo),
                BirthDate = new DateTime(1972, 10, 17) // This guy is old ;)
            });
        }
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="organizationName"></param>
/// <returns></returns>
///===============================================================================================================================
        private Address CreateAddress(string organizationName, ShsRepository repo)
        {
            return repo.Create<Address>(() => new Address
            {
                City       = "Tomball",
                State      = "TX",
                PostalCode = "22341",
                Line       = organizationName + " Lane",
                Use        = Use.Work,
                Country    = "USA"
            });
        } 
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
///===============================================================================================================================
        private ObservableCollection<Person.Person> CreatePeople(Organization organization, ShsRepository repo)
        {
             var people = new ObservableCollection<Person.Person>();

            for (int i = 0; i < CREATE_USER_COUNT; i++)
            {
                var newUser = repo.Create(() => new User
                {
                    Organization = organization,
                    Actor        = this.CreateOrganizationContact(organization.Name, repo),
                    Email        = "User" + Convert.ToString(i) + "@" + organization.Name + ".com",
                    Password     = "JustFl0ck!t",
                    Name         = "User" + Convert.ToString(i)
                });

                var newPractitioner = repo.Create(() => new Practitioner
                {
                    Organization    = organization,
                    Role            = PractitionerRole.Pharmacist,
                    SecurityDetails = newUser,
                    Specialty       = PractitionerSpecialty.DietaryConsultant,
                    Name            = new HumanName("User" + Convert.ToString(i), organization.Name),
                    Address         = this.CreateAddress(organization.Name, repo),
                    BirthDate       = new DateTime(1972, 10, 17)
                });

                newPractitioner.ContactMethods.Add(repo.Create(() => new ContactMethod(ContactSystem.Email, ContactUse.Work, "User" + Convert.ToString(i) + "@" + organization.Name + ".com")));
                people.Add(newPractitioner);
        }

            // We also include at least one Admin user..

            var adminUser = repo.Create(() => new User
            {
                Organization = organization,
                Actor        = this.CreateOrganizationContact(organization.Name, repo),
                Email        = "Admin@" + organization.Name + ".com",
                Password     = "JustFl0ck!t",
                Name         = "Admin"
            });

            var newAdminPractitioner = repo.Create(() => new Practitioner
            {
                Organization    = organization,
                Role            = PractitionerRole.Ict,
                SecurityDetails = adminUser,
                Specialty       = PractitionerSpecialty.SystemsArchitect,
                Name            = new HumanName("AdminUser", organization.Name),
                Address         = this.CreateAddress(organization.Name, repo),
                BirthDate       = new DateTime(1972, 10, 17)
            });

            people.Add(newAdminPractitioner);
            return people;
        }
//===============================================================================================================================
///  <summary>
///  
///  </summary>
///  <param name=""></param>
/// <param name="userType"></param>
/// ===============================================================================================================================
        private List<Claim> GetClaims(UserType userType)
        {
            var claims = new List<Claim>();

            switch (userType)
            {
                case (UserType.Admin):
                break;

                case (UserType.User):
                break;
            }
            return claims;
        }
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
///===============================================================================================================================
        [TestMethod]
        public void SeedData()
        {
           this.SeedOganizations();
        }
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
///===============================================================================================================================
        [TestInitialize]
        public void Setup()
        {
            if (!_isSetup)
            {
                //DbRunner.StartOrientDb(@"C:\OrientDb", @"C:\Program Files\Java\jre1.8.0_31");
                var databaseName = "SimplicityHealth";
                
                OClient.CreateDatabasePool("127.0.0.1",
                    2424,
                    databaseName,
                    ODatabaseType.Graph,
                    "admin",
                    "admin",
                    10,
                    CONNECTION_NAME);
                _isSetup = true;
            }
        }
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
///===============================================================================================================================
        [TestCleanup]
        public void TearDown()
        {
            //DbRunner.StopOrientDb();
        }
    }
}
