using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orient.Client;
using SHS.Sage.OrientDb.Tests.Associations;
using SHS.Sage.OrientDb.Tests.Things;
using SHS.Sage.Linq;
using System.Reflection;
using SHS.Sage.Linq.Runtime;
using System.Collections.Generic;

namespace SHS.Sage.OrientDb.Tests
{
    [TestClass]
    public class RepositoryTests
    {
        private const string CONNECTION_NAME = "TDB";

        #region Reading/Writing Tests
        #region ExecuteXXX methods
        [TestMethod]
        public void CanExecuteRawReader()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var reader = repo.ExecuteReader("SELECT FROM Patient LIMIT 1");
            reader.Read();
            var id = reader.GetString(reader.GetOrdinal("Id"));
            Assert.IsTrue(!string.IsNullOrEmpty(id));
        }

        [TestMethod]
        public void CanExecuteRawTypeEnumerable()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var identifiable = repo.Create<OrthopedicPatient>((p) => { p.Age = 3; p.Name = "Pat"; p.Joint = "Knee"; });
            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            var results = repo.ExecuteEnumerable<Patient>(
                "SELECT *, @class FROM Patient LIMIT 10").ToArray();

            Assert.IsTrue(results.Count() > 0);
        }

        [TestMethod]
        public void CanExecuteRawBaseTypeEnumerable()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var identifiable = repo.Create<OrthopedicPatient>((p) => { p.Age = 3; p.Name = "Pat"; p.Joint = "Knee"; });
            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            var results = repo.ExecuteEnumerable<Patient>(
                "SELECT FROM Patient LIMIT 10").ToArray();

            Assert.IsTrue(results.Count() > 0);
        }
        #endregion

        #region Things

        #region Insert tests
        [TestMethod]
        public void CanCreateSimpleIIdentifiableType()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat"; }) as IIdentifiable;
            Assert.IsTrue(identifiable is IProxyIdentifiable);

            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            Assert.IsTrue(((Patient)patient).Id != null, "Id should not be null");
            Assert.IsTrue(((Patient)patient).Age == ((Patient)identifiable).Age, "Age is incorrect");
            Assert.IsTrue(((Patient)patient).Name == ((Patient)identifiable).Name, "Name is incorrect");
            Assert.IsTrue(ReferenceEquals(identifiable, patient));
        }

        [TestMethod]
        public void CanCreateSimpleGenericType()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat"; });
            Assert.IsTrue(identifiable is IProxyIdentifiable);

            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            Assert.IsTrue(patient.Id != null, "Id should not be null");
            Assert.IsTrue(patient.Age == identifiable.Age, "Age is incorrect");
            Assert.IsTrue(patient.Name == identifiable.Name, "Name is incorrect");
            Assert.IsTrue(ReferenceEquals(identifiable, patient));
        }

        [TestMethod]
        public void CanCreateSubtypedIIdentifiableType()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<OrthopedicPatient>((p) => { p.Age = 3; p.Name = "Pat"; p.Joint = "Knee"; });
            Assert.IsTrue(identifiable is IProxyIdentifiable);

            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            Assert.IsTrue(((OrthopedicPatient)patient).Id != null, "Id should not be null");
            Assert.IsTrue(((OrthopedicPatient)patient).Age == ((OrthopedicPatient)identifiable).Age, "Age is incorrect");
            Assert.IsTrue(((OrthopedicPatient)patient).Name == ((OrthopedicPatient)identifiable).Name, "Name is incorrect");
            Assert.IsTrue(((OrthopedicPatient)patient).Joint == ((OrthopedicPatient)identifiable).Joint, "Joint is incorrect");
            Assert.IsTrue(ReferenceEquals(identifiable, patient));
        }

        [TestMethod]
        public void CanCreateSubtypedGenericType()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<OrthopedicPatient>((p) => { p.Age = 3; p.Name = "Pat"; p.Joint = "Knee"; });
            Assert.IsTrue(identifiable is IProxyIdentifiable);

            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            Assert.IsTrue(patient.Id != null, "Id should not be null");
            Assert.IsTrue(patient.Age == identifiable.Age, "Age is incorrect");
            Assert.IsTrue(patient.Name == identifiable.Name, "Name is incorrect");
            Assert.IsTrue(patient.Joint == identifiable.Joint, "Joint is incorrect");
            Assert.IsTrue(ReferenceEquals(identifiable, patient));
        }

        [TestMethod]
        public void CanCreateMultipleSimpleTypes()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var p1 = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat"; });
            var p2 = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat"; });
            var p3 = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat"; });
            var p4 = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat"; });

            repo.SaveChanges();

            var patient1 = repo.Get(p1);
            var patient2 = repo.Get(p2);
            var patient3 = repo.Get(p3);
            var patient4 = repo.Get(p4);

            Assert.IsTrue(repo.Count() == 4);
            Assert.IsTrue(!string.IsNullOrEmpty(patient1.Id));
            Assert.IsTrue(!string.IsNullOrEmpty(patient2.Id));
            Assert.IsTrue(!string.IsNullOrEmpty(patient3.Id));
            Assert.IsTrue(!string.IsNullOrEmpty(patient4.Id));
        }


        #endregion

        #region Update tests
        [TestMethod]
        public void CanUpdateSimpleIIdentifiableType()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            
            var id = GetFirstId(repo, "Patient");
            
            var patient = repo.Get(id);
            ((Patient)patient).Age = 5;
            repo.SaveChanges();

            repo.ClearChanges(); // make sure we're not getting a cached instance

            patient = repo.Get<Patient>(id);

            Assert.IsTrue(((Patient)patient).Age == 5);

        }

        [TestMethod]
        public void CanUpdateSimpleGenericType()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            
            var id = GetFirstId(repo, "Patient");

            var patient = repo.Get<Patient>(id);
            patient.Age = 15;
            repo.SaveChanges();

            repo.ClearChanges(); // make sure we're not getting a cached instance

            patient = repo.Get<Patient>(id);

            Assert.IsTrue(patient.Age == 15);
        }

        #endregion

        #region Complex Insert/Update Combinations
        [TestMethod]
        public void CanInsertParentAndInsertComplexChildProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var id = GetFirstId(repo, "Patient");

            // create a new patient, create a new address, assign it to patient, and save them both
            var patient = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var address = repo.Create<Address>((a) => { a.City = "Test"; a.Postal = "12345"; a.State = "TX"; a.Street1 = "Street"; });
            patient.Address = address;

            repo.SaveChanges();

            Assert.IsTrue(ReferenceEquals(patient.Address, address));

            repo.ClearChanges();

            patient = repo.Get<Patient>(id);

            Assert.IsTrue(patient.Address != null);
        }

        [TestMethod]
        public void CanUpdateParentAndInsertComplexChildProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var id = GetFirstId(repo, "Patient");

            // get an existing patient, create a new address, assign it to patient, and save them both
            var patient = repo.Get<Patient>(id);
            var address = repo.Create<Address>((a) => { a.City = "Test"; a.Postal = "12345"; a.State = "TX"; a.Street1 = "Street"; });
            patient.Address = address;

            repo.SaveChanges();

            Assert.IsTrue(ReferenceEquals(patient.Address, address));

            repo.ClearChanges();

            patient = repo.Get<Patient>(id);

            Assert.IsTrue(patient.Address != null);
        }

        [TestMethod]
        public void CanInsertCodependentSiblings()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            // create three new patients that all depend on each other
            var stan = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var jane = repo.Create<Patient>((p) => { p.Name = "Jane"; p.Age = 10; });
            var jess = repo.Create<Patient>((p) => { p.Name = "Jess"; p.Age = 10; });

            stan.Sibling = jane;
            jane.Sibling = stan;
            jess.Sibling = stan;

            repo.SaveChanges(); // should produce 3 inserts and 1 update call

            Assert.IsTrue(ReferenceEquals(stan.Sibling, jane));
            Assert.IsTrue(ReferenceEquals(jane.Sibling, stan));
            Assert.IsTrue(ReferenceEquals(jess.Sibling, stan));

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif

            repo.ClearChanges();

            stan = repo.Get<Patient>(stan.Id);
            jane = repo.Get<Patient>(jane.Id);
            jess = repo.Get<Patient>(jess.Id);

            Assert.IsTrue(ReferenceEquals(stan.Sibling, jane));
            Assert.IsTrue(ReferenceEquals(jane.Sibling, stan));
            Assert.IsTrue(ReferenceEquals(jess.Sibling, stan));
        }

        [TestMethod]
        public void CanInsertParentAndInsertArrayChildProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.PatientsArray = new Patient[] { child1, child2 };

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.PatientsArray[0], child1));
            Assert.IsTrue(ReferenceEquals(parent.PatientsArray[1], child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsArray.Length == 2);
            Assert.IsTrue(parent.PatientsArray[0].Name == "Child1");
            Assert.IsTrue(parent.PatientsArray[1].Name == "Child2");
        }

        [TestMethod]
        public void CanInsertOrUpdateParentAndInsertAndRemoveChildFromArrayProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.PatientsArray = new Patient[] { child1, child2 };

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.PatientsArray[0], child1));
            Assert.IsTrue(ReferenceEquals(parent.PatientsArray[1], child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsArray.Length == 2);
            Assert.IsTrue(parent.PatientsArray[0].Name == "Child1");
            Assert.IsTrue(parent.PatientsArray[1].Name == "Child2");

            // array elements are ordinally insensitive, so we intentionally move things around
            parent.PatientsArray = new Patient[] 
            {
                repo.Create<Patient>((p) => { p.Name = "Child3"; p.Age = 10; }),
                parent.PatientsArray[0]
            };

            repo.SaveChanges();

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 1);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(parent.PatientsArray.Length == 2);

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsArray.Length == 2);
            Assert.IsTrue(parent.PatientsArray.Any(c => c.Name.Equals("Child1")));
            Assert.IsTrue(parent.PatientsArray.Any(c => c.Name.Equals("Child3")));
        }

        [TestMethod]
        public void CanInsertParentAndInsertIListChildProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.PatientsIList = (IList<Patient>)(new Patient[] { child1, child2 });

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.PatientsIList[0], child1));
            Assert.IsTrue(ReferenceEquals(parent.PatientsIList[1], child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsIList.Count == 2);
            Assert.IsTrue(parent.PatientsIList[0].Name == "Child1");
            Assert.IsTrue(parent.PatientsIList[1].Name == "Child2");
        }

        [TestMethod]
        public void CanInsertOrUpdateParentAndInsertAndRemoveChildFromIListProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.PatientsIList = (IList<Patient>)(new Patient[] { child1, child2 });

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.PatientsIList[0], child1));
            Assert.IsTrue(ReferenceEquals(parent.PatientsIList[1], child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsIList.Count == 2);
            Assert.IsTrue(parent.PatientsIList[0].Name == "Child1");
            Assert.IsTrue(parent.PatientsIList[1].Name == "Child2");

            // array elements are ordinally insensitive, so we intentionally move things around
            parent.PatientsIList = (IList<Patient>)(new Patient[]
            {
                repo.Create<Patient>((p) => { p.Name = "Child3"; p.Age = 10; }),
                parent.PatientsIList[0]
            });

            repo.SaveChanges();

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 1);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(parent.PatientsIList.Count == 2);

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsIList.Count == 2);
            Assert.IsTrue(parent.PatientsIList.Any(c => c.Name.Equals("Child1")));
            Assert.IsTrue(parent.PatientsIList.Any(c => c.Name.Equals("Child3")));
        }

        [TestMethod]
        public void CanInsertParentAndInsertListChildProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.PatientsList = new List<Patient>((new Patient[] { child1, child2 }));

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.PatientsList[0], child1));
            Assert.IsTrue(ReferenceEquals(parent.PatientsList[1], child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsList.Count == 2);
            Assert.IsTrue(parent.PatientsList[0].Name == "Child1");
            Assert.IsTrue(parent.PatientsList[1].Name == "Child2");
        }

        [TestMethod]
        public void CanInsertOrUpdateParentAndInsertAndRemoveChildFromListProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.PatientsList = new List<Patient>((new Patient[] { child1, child2 }));

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.PatientsList[0], child1));
            Assert.IsTrue(ReferenceEquals(parent.PatientsList[1], child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsList.Count == 2);
            Assert.IsTrue(parent.PatientsList[0].Name == "Child1");
            Assert.IsTrue(parent.PatientsList[1].Name == "Child2");

            // array elements are ordinally insensitive, so we intentionally move things around
            parent.PatientsList = new List<Patient>(new Patient[]
            {
                repo.Create<Patient>((p) => { p.Name = "Child3"; p.Age = 10; }),
                parent.PatientsList[0]
            });

            repo.SaveChanges();

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 1);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(parent.PatientsList.Count == 2);

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsList.Count == 2);
            Assert.IsTrue(parent.PatientsList.Any(c => c.Name.Equals("Child1")));
            Assert.IsTrue(parent.PatientsList.Any(c => c.Name.Equals("Child3")));
        }

        [TestMethod]
        public void CanInsertParentAndInsertIEnumerableChildProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.PatientsIEnumerable = new List<Patient>((new Patient[] { child1, child2 })).AsEnumerable();

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.PatientsIEnumerable.ElementAt(0), child1));
            Assert.IsTrue(ReferenceEquals(parent.PatientsIEnumerable.ElementAt(1), child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsIEnumerable.Count() == 2);
            Assert.IsTrue(parent.PatientsIEnumerable.ElementAt(0).Name == "Child1");
            Assert.IsTrue(parent.PatientsIEnumerable.ElementAt(1).Name == "Child2");
        }

        [TestMethod]
        public void CanInsertOrUpdateParentAndInsertAndRemoveChildFromIEnumerableProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.PatientsIEnumerable = new List<Patient>((new Patient[] { child1, child2 })).AsEnumerable();

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.PatientsIEnumerable.ElementAt(0), child1));
            Assert.IsTrue(ReferenceEquals(parent.PatientsIEnumerable.ElementAt(1), child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsIEnumerable.Count() == 2);
            Assert.IsTrue(parent.PatientsIEnumerable.ElementAt(0).Name == "Child1");
            Assert.IsTrue(parent.PatientsIEnumerable.ElementAt(1).Name == "Child2");

            // array elements are ordinally insensitive, so we intentionally move things around
            parent.PatientsIEnumerable = new List<Patient>(new Patient[]
            {
                repo.Create<Patient>((p) => { p.Name = "Child3"; p.Age = 10; }),
                parent.PatientsIEnumerable.ElementAt(0)
            }).AsEnumerable();

            repo.SaveChanges();

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 1);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(parent.PatientsIEnumerable.Count() == 2);

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.PatientsIEnumerable.Count() == 2);
            Assert.IsTrue(parent.PatientsIEnumerable.Any(c => c.Name.Equals("Child1")));
            Assert.IsTrue(parent.PatientsIEnumerable.Any(c => c.Name.Equals("Child3")));
        }

        [TestMethod]
        public void CanInsertParentIIdentifiableAndInsertIEnumerableIIdentifiableChildProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.IdentifiableEnumerable = new List<IIdentifiable>((new IIdentifiable[] { child1, child2 })).AsEnumerable();

            repo.SaveChanges();
#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.IdentifiableEnumerable.ElementAt(0), child1));
            Assert.IsTrue(ReferenceEquals(parent.IdentifiableEnumerable.ElementAt(1), child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.IdentifiableEnumerable.Count() == 2);
            Assert.IsTrue(((Patient)parent.IdentifiableEnumerable.ElementAt(0)).Name == "Child1");
            Assert.IsTrue(((Patient)parent.IdentifiableEnumerable.ElementAt(1)).Name == "Child2");
        }

        [TestMethod]
        public void CanInsertOrUpdateParentIIdentifiableAndInsertAndRemoveChildFromIEnumerableIIdentifiableProperty()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child1 = repo.Create<Patient>((p) => { p.Name = "Child1"; p.Age = 10; });
            var child2 = repo.Create<Patient>((p) => { p.Name = "Child2"; p.Age = 10; });

            parent.IdentifiableEnumerable = new List<IIdentifiable>((new IIdentifiable[] { child1, child2 })).AsEnumerable();

            repo.SaveChanges();

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 3);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(ReferenceEquals(parent.IdentifiableEnumerable.ElementAt(0), child1));
            Assert.IsTrue(ReferenceEquals(parent.IdentifiableEnumerable.ElementAt(1), child2));

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.IdentifiableEnumerable.Count() == 2);
            Assert.IsTrue(((Patient)parent.IdentifiableEnumerable.ElementAt(0)).Name == "Child1");
            Assert.IsTrue(((Patient)parent.IdentifiableEnumerable.ElementAt(1)).Name == "Child2");

            // array elements are ordinally insensitive, so we intentionally move things around
            parent.IdentifiableEnumerable = new List<IIdentifiable>(new IIdentifiable[]
            {
                repo.Create<Patient>((p) => { p.Name = "Child3"; p.Age = 10; }),
                parent.IdentifiableEnumerable.ElementAt(0)
            }).AsEnumerable();

            repo.SaveChanges();

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 1);
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Update) == 1);
#endif
            Assert.IsTrue(parent.IdentifiableEnumerable.Count() == 2);

            repo.ClearChanges();

            parent = repo.Get(parent);

            Assert.IsTrue(parent.IdentifiableEnumerable.Count() == 2);
            Assert.IsTrue(parent.IdentifiableEnumerable.OfType<Patient>().Any(c => c.Name.Equals("Child1")));
            Assert.IsTrue(parent.IdentifiableEnumerable.OfType<Patient>().Any(c => c.Name.Equals("Child3")));
        }

        [TestMethod]
        public void CanInsertUntrackedScalarComplexDescendants()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            var child = new Patient() { Name = "Different" };
            parent.Sibling = child; // new Patient() { Name = "Untracked" };
            Assert.IsFalse(ReferenceEquals(child, parent.Sibling));
            repo.SaveChanges();

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 2);
#endif
            Assert.IsTrue(parent.Sibling is IProxyIdentifiable);
        }

        [TestMethod]
        public void CanInsertUntrackedEnumerableComplexDescendants()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var parent = repo.Create<Patient>((p) => { p.Name = "Stan"; p.Age = 10; });
            parent.PatientsArray = new Patient[] { new Patient() { Name = "Untracked" } };
            repo.SaveChanges();

#if (DEBUG)
            Assert.IsTrue(repo.Operations.Count(o => o.OpetrationType == OperationType.Insert) == 2);
#endif
            Assert.IsTrue(parent.PatientsArray[0] is IProxyIdentifiable);
        }
        #endregion

        #region Get item tests
        [TestMethod]
        public void CanGetComplexGenericTypeFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var patientId = GetFirstId(repo, "Patient");
            var addressId = GetFirstId(repo, "Address");

            repo.ExecuteStatement("UPDATE " + patientId + " Set Address=" + addressId);

            var patient = repo.Get<Patient>(new Patient() { Id = patientId });
            var address = patient.Address;
            Assert.IsTrue(address != null && address.Id == addressId);
        }

        [TestMethod]
        public void CanPolicySuppressGetComplexGenericTypeFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            repo.Policy.DeferLoadComplexTypes = false; // turn off lazy loading

            var patientId = GetFirstId(repo, "Patient");
            var addressId = GetFirstId(repo, "Address");

            repo.ExecuteStatement("UPDATE " + patientId + " Set Address=" + addressId);

            var patient = repo.Get<Patient>(new Patient() { Id = patientId });
            var address = patient.Address;
            Assert.IsTrue(address == null, "Address should not load");
        }
        [TestMethod]
        public void CanGetTypeByStringId()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var id = GetFirstId(repo, "Patient");
            var patient = repo.Get(id);
            Assert.IsTrue(patient is Patient);
        }

        [TestMethod]
        public void CanGetSimpleIIdentifiableFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var patient = new Patient();

            using (var reader = repo.ExecuteReader("SELECT FROM Patient LIMIT 1"))
            {
                reader.Read();
                patient.Id = reader.GetString(reader.GetOrdinal("Id"));
                patient.Name = reader.GetString(reader.GetOrdinal("Name"));
                patient.Age = reader.GetInt32(reader.GetOrdinal("Age"));
            }

            var emptyPatient = new Patient() { Id = patient.Id } as IIdentifiable;

            var getPatient = repo.Get(emptyPatient);

            Assert.IsTrue(patient.Id == getPatient.Id, "Ids are not equal");
            Assert.IsTrue(((Patient)patient).Age == ((Patient)getPatient).Age, "Age is incorrect");
            Assert.IsTrue(((Patient)patient).Name == ((Patient)getPatient).Name, "Name is incorrect");
            //Assert.IsTrue(((Patient)patient) is IGraph<Patient>);
        }

        [TestMethod]
        public void CanGetSimpleGenericTypeFromThingSet()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = new Patient();

            using (var reader = repo.ExecuteReader("SELECT FROM Patient LIMIT 1"))
            {
                reader.Read();
                patient.Id = reader.GetString(reader.GetOrdinal("Id"));
                patient.Name = reader.GetString(reader.GetOrdinal("Name"));
                patient.Age = reader.GetInt32(reader.GetOrdinal("Age"));
            }

            var emptyPatient = new Patient() { Id = patient.Id };

            var getPatient = repo.Patients.Get(emptyPatient);

            Assert.IsTrue(patient.Id == getPatient.Id, "Ids are not equal");
            Assert.IsTrue(patient.Age == getPatient.Age, "Age is incorrect");
            Assert.IsTrue(patient.Name == getPatient.Name, "Name is incorrect");
            //Assert.IsTrue(patient is IGraph<Patient>);
        }

        [TestMethod]
        public void CanRepoDisposedSuppressGetComplexGenericTypeFromRepo()
        {
            Patient patient;

            using (var repo = new HealthRepository(new ODatabase(CONNECTION_NAME)))
            {
                var id = GetFirstId(repo, "Patient");
                patient = repo.Get<Patient>(new Patient() { Id = id });
            }

            var address = patient.Address;
            Assert.IsTrue(address == null, "Address should not load");
        }

        [TestMethod]
        public void CanGetSimpleGenericTypeFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var patient = new Patient();

            using (var reader = repo.ExecuteReader("SELECT FROM Patient LIMIT 1"))
            {
                reader.Read();
                patient.Id = reader.GetString(reader.GetOrdinal("Id"));
                patient.Name = reader.GetString(reader.GetOrdinal("Name"));
                patient.Age = reader.GetInt32(reader.GetOrdinal("Age"));
            }

            var emptyPatient = new Patient() { Id = patient.Id };
            var getPatient = repo.Get(emptyPatient);

            Assert.IsTrue(patient.Id == getPatient.Id, "Ids are not equal");
            Assert.IsTrue(patient.Age == getPatient.Age, "Age is incorrect");
            Assert.IsTrue(patient.Name == getPatient.Name, "Name is incorrect");
            //Assert.IsTrue(patient is IGraph<Patient>);
        }
        #endregion

        #region Delete tests
        [TestMethod]
        public void CanDeleteIdsFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient1 = new Patient() { Age = 3, Name = "Pat" + Environment.TickCount.ToString() };
            var patient2 = new Patient() { Age = 3, Name = "Pat" + Environment.TickCount.ToString() };

            patient1 = repo.Attach(patient1);
            patient2 = repo.Attach(patient2);

            repo.SaveChanges();

            var id1 = patient1.Id;
            var id2 = patient2.Id;

            var tracked1 = repo.Delete(patient1.Id);
            var tracked2 = repo.Delete(patient2.Id);

            Assert.IsTrue(repo.Count() == 2);

            repo.SaveChanges();

            var found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + id1))
            {
                found = reader.Read();
            }

            Assert.IsTrue(found == false, "Record was not deleted");

            using (var reader = repo.ExecuteReader("SELECT FROM " + id2))
            {
                found = reader.Read();
            }

            Assert.IsTrue(found == false, "Record was not deleted");
            Assert.IsTrue(repo.Count() == 0);
            Assert.IsTrue(tracked1.Id == "#Deleted");
            Assert.IsTrue(tracked2.Id == "#Deleted");
        }

        [TestMethod]
        public void CanDeleteSimpleGenericTypeFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = new Patient() { Age = 3, Name = "Pat" + Environment.TickCount.ToString() };
            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Patient SET Name='{0}', Age={1} RETURN @this", patient.Name, patient.Age)))
            {
                reader.Read();
                patient.Id = reader.GetString(reader.GetOrdinal("Id"));
                patient.Name = reader.GetString(reader.GetOrdinal("Name"));
                patient.Age = reader.GetInt32(reader.GetOrdinal("Age"));
            }

            var emptyPatient = new Patient() { Id = patient.Id };
            emptyPatient = repo.Delete(emptyPatient);
            repo.SaveChanges();

            var found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + patient.Id))
            {
                found = reader.Read();
            }

            Assert.IsTrue(found == false, "Record was not deleted");
            Assert.IsTrue(repo.Any() == false);
            Assert.IsTrue(emptyPatient.Id == "#Deleted");
        }

        [TestMethod]
        public void CanDeleteSimpleIIdentifiableFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = new Patient() { Age = 3, Name = "Pat" + Environment.TickCount.ToString() };
            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Patient SET Name='{0}', Age={1} RETURN @this", patient.Name, patient.Age)))
            {
                reader.Read();
                patient.Id = reader.GetString(reader.GetOrdinal("Id"));
                patient.Name = reader.GetString(reader.GetOrdinal("Name"));
                patient.Age = reader.GetInt32(reader.GetOrdinal("Age"));
            }

            var emptyPatient = new Patient() { Id = patient.Id } as IIdentifiable;
            emptyPatient = repo.Delete(emptyPatient);
            repo.SaveChanges();

            var found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + patient.Id))
            {
                found = reader.Read();
            }

            Assert.IsTrue(found == false, "Record was not deleted");
            Assert.IsTrue(repo.Any() == false);
            Assert.IsTrue(emptyPatient.Id == "#Deleted");
        }

        [TestMethod]
        public void CanNotDeferLoadComplexTypesAfterDelete()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var age = 3;
            var state = "TX";
            var country = "USA";
            string countryId, addressId, patientId;

            #region Seed Data
            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Country SET Name2='{0}' RETURN @this", country)))
            {
                reader.Read();
                countryId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Address SET Street1='{0}', Street2='{1}', City='{2}', State='{3}', Postal='{4}', Country={5} RETURN @this",
                "1 Main Street",
                null,
                "Tomball",
                state,
                "77355",
                countryId)))
            {
                reader.Read();
                addressId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Patient SET Name='{0}', Age={1}, Address={2} RETURN @this",
                "Patient",
                age,
                addressId)))
            {
                reader.Read();
                patientId = reader.GetString(reader.GetOrdinal("Id"));
            }
            #endregion
            
            var result = repo.Patients.Where(p =>
                    p.Age == age
                    && p.Address.State == state
                    && p.Address.Country.Name.Equals(country))
                .FirstOrDefault();

            result = repo.Delete(result);
            repo.SaveChanges();

            var addressEnt = result.Address; // deferred loading should be turned off by the delete
            var countryEnt = addressEnt == null ? null : addressEnt.Country;

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Name != null);
            Assert.IsTrue(addressEnt == null);
            Assert.IsTrue(countryEnt == null);
            Assert.IsTrue(result.Id == "#Deleted");
        }

        [TestMethod]
        public void CanCascadeDeferredLoadablesDuringDelete()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var age = 3;
            var state = "TX";
            var country = "USA";
            string countryId, addressId, patientId;

            #region Seed Data
            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Country SET Name='{0}' RETURN @this", country)))
            {
                reader.Read();
                countryId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Address SET Street1='{0}', Street2='{1}', City='{2}', State='{3}', Postal='{4}', Country={5} RETURN @this",
                "1 Main Street",
                null,
                "Tomball",
                state,
                "77355",
                countryId)))
            {
                reader.Read();
                addressId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Patient SET Name='{0}', Age={1}, Address={2} RETURN @this",
                "Patient",
                age,
                addressId)))
            {
                reader.Read();
                patientId = reader.GetString(reader.GetOrdinal("Id"));
            }
            #endregion

            var result = repo.Get<Patient>(patientId);
            result = repo.Delete(result, true); // cascade the delete
            repo.SaveChanges();

            var addressEnt = result.Address; // deferred loading should be turned off by the delete
            var countryEnt = addressEnt == null ? null : addressEnt.Country;

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Name != null);
            Assert.IsTrue(addressEnt != null); // it will be loaded during cascade
            Assert.IsTrue(countryEnt != null); // it will be loaded during cascade
            Assert.IsTrue(result.Id == "#Deleted");
            Assert.IsTrue(addressEnt.Id == "#Deleted");
            Assert.IsTrue(countryEnt.Id == "#Deleted");

            var found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + patientId))
            {
                found = reader.Read();
            }
            Assert.IsTrue(found == false);

            found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + addressId))
            {
                found = reader.Read();
            }
            Assert.IsTrue(found == false);

            found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + countryId))
            {
                found = reader.Read();
            }
            Assert.IsTrue(found == false);
        }

        [TestMethod]
        public void CanNotDeleteByIdWhenPolicyTrackChangesIsFalse()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            repo.Policy.TrackChanges = false; // prevents all change processing
            try
            {
                repo.Delete("#13:1");
            }
            catch(InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }
        #endregion

        #endregion

        #region Associations

        #region Saving tests
        [TestMethod]
        public void CanCreateComplexAssociation()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var source = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Source"; }) as IThing;
            var target = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Target"; }) as IThing;
            var preferred = repo.Create<Patient>((p) => { p.Age = 5; p.Name = "Preferred"; });

            var doctors = repo.Create<doctors>((d) => { d.Source = source; d.Target = target; d.PreferredPatient = preferred; });

            repo.SaveChanges();

            Assert.IsTrue(ReferenceEquals(doctors.Source, source));
            Assert.IsTrue(ReferenceEquals(doctors.Target, target));
            Assert.IsTrue(ReferenceEquals(doctors.PreferredPatient, preferred));
            Assert.IsTrue(source.Id != null);
            Assert.IsTrue(target.Id != null);
            Assert.IsTrue(preferred.Id != null);

            repo.ClearChanges();

            doctors = repo.Get<doctors>(doctors.Id);

            Assert.IsTrue(doctors.Source.Id != null);
            Assert.IsTrue(doctors.Target.Id != null);
            Assert.IsTrue(doctors.PreferredPatient.Id != null);
        }

        [TestMethod]
        public void CanUpdateComplexAssociation()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var source = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Source"; }) as IThing;
            var target = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Target"; }) as IThing;
            var preferred = repo.Create<Patient>((p) => { p.Age = 5; p.Name = "Preferred"; });

            var doctors = repo.Create<doctors>((d) => { d.Source = source; d.Target = target; });

            repo.SaveChanges();

            doctors.PreferredPatient = preferred;

            repo.SaveChanges();

            Assert.IsTrue(ReferenceEquals(doctors.Source, source));
            Assert.IsTrue(ReferenceEquals(doctors.Target, target));
            Assert.IsTrue(ReferenceEquals(doctors.PreferredPatient, preferred));
            Assert.IsTrue(source.Id != null);
            Assert.IsTrue(target.Id != null);
            Assert.IsTrue(preferred.Id != null);

            repo.ClearChanges();

            doctors = repo.Get<doctors>(doctors.Id);

            Assert.IsTrue(doctors.Source.Id != null);
            Assert.IsTrue(doctors.Target.Id != null);
            Assert.IsTrue(doctors.PreferredPatient.Id != null);
        }

        #endregion

        #region Delete tests
        [TestMethod]
        public void CanDeleteAssociationIdsFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var source = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Source"; }) as IThing;
            var target = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Target"; }) as IThing;
            var preferred = repo.Create<Patient>((p) => { p.Age = 5; p.Name = "Preferred"; });

            var doctors = repo.Create<doctors>((d) => { d.Source = source; d.Target = target; d.PreferredPatient = preferred; });

            repo.SaveChanges();
            repo.ClearChanges();

            var id1 = doctors.Id;

            var tracked1 = repo.Delete(doctors.Id);

            Assert.IsTrue(repo.Count() == 1);

            repo.SaveChanges();

            var found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + id1))
            {
                found = reader.Read();
            }

            Assert.IsTrue(found == false, "Record was not deleted");

            Assert.IsTrue(repo.Count() == 0);
            Assert.IsTrue(tracked1.Id == "#Deleted");
        }

        [TestMethod]
        public void CanDeleteSimpleGenericAssociationTypeFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var source = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Source"; }) as IThing;
            var target = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Target"; }) as IThing;
            var preferred = repo.Create<Patient>((p) => { p.Age = 5; p.Name = "Preferred"; });

            var doctors = repo.Create<doctors>((d) => { d.Source = source; d.Target = target; d.PreferredPatient = preferred; });

            repo.SaveChanges();
            repo.ClearChanges();

            var id1 = doctors.Id;

            var tracked1 = repo.Delete(doctors);

            Assert.IsTrue(repo.Count() == 4);

            repo.SaveChanges();

            var found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + id1))
            {
                found = reader.Read();
            }

            Assert.IsTrue(found == false, "Record was not deleted");

            Assert.IsTrue(repo.Count() == 3);
            Assert.IsTrue(tracked1.Id == "#Deleted");
        }

        [TestMethod]
        public void CanDeleteSimpleIIdentifiableAssociationFromRepo()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var source = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Source"; }) as IThing;
            var target = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Target"; }) as IThing;
            var preferred = repo.Create<Patient>((p) => { p.Age = 5; p.Name = "Preferred"; });

            var doctors = repo.Create<doctors>((d) => { d.Source = source; d.Target = target; d.PreferredPatient = preferred; });

            repo.SaveChanges();
            repo.ClearChanges();

            var id1 = doctors.Id;
            var emptyDoctors = new doctors() { Id = id1 } as IIdentifiable;
            var tracked1 = repo.Delete(emptyDoctors);

            Assert.IsTrue(repo.Count() == 1);

            repo.SaveChanges();

            var found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + id1))
            {
                found = reader.Read();
            }

            Assert.IsTrue(found == false, "Record was not deleted");

            Assert.IsTrue(repo.Count() == 0);
            Assert.IsTrue(tracked1.Id == "#Deleted");
        }

        [TestMethod]
        public void CanNotDeferLoadComplexAssociationTypesAfterDelete()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var source = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Source"; }) as IThing;
            var target = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Target"; }) as IThing;
            var preferred = repo.Create<Patient>((p) => { p.Age = 5; p.Name = "Preferred"; });

            var doctors = repo.Create<doctors>((d) => { d.Source = source; d.Target = target; d.PreferredPatient = preferred; });

            repo.SaveChanges();
            repo.ClearChanges();

            var deferLoaded = repo.Get<doctors>(doctors.Id); // links won't be loaded 

            var tracked1 = repo.Delete(deferLoaded);
            repo.SaveChanges();

            Assert.IsTrue(deferLoaded.PreferredPatient == null);
            Assert.IsTrue(deferLoaded.Source == null);
            Assert.IsTrue(deferLoaded.Target == null);
            Assert.IsTrue(tracked1.Id == "#Deleted");
        }

        [TestMethod]
        public void CanCascadeDeferredLoadablesAssociationDuringDelete()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var source = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Source"; }) as IThing;
            var target = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Target"; }) as IThing;
            var preferred = repo.Create<Patient>((p) => { p.Age = 5; p.Name = "Preferred"; });

            var doctors = repo.Create<doctors>((d) => { d.Source = source; d.Target = target; d.PreferredPatient = preferred; });

            repo.SaveChanges();

            var sourceId = source.Id;
            var targetId = target.Id;
            var preferredId = preferred.Id;

            repo.ClearChanges();

            var deferLoaded = repo.Get<doctors>(doctors.Id); // links will be defer loaded 

            var tracked1 = repo.Delete(deferLoaded, true);
            repo.SaveChanges();
            repo.ClearChanges();

            var found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + sourceId))
            {
                found = reader.Read();
            }
            Assert.IsFalse(found);

            found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + targetId))
            {
                found = reader.Read();
            }
            Assert.IsFalse(found);

            found = true;
            using (var reader = repo.ExecuteReader("SELECT FROM " + preferredId))
            {
                found = reader.Read();
            }
            Assert.IsFalse(found);

            Assert.IsTrue(tracked1.Id == "#Deleted");
        }

        [TestMethod]
        public void CanNotDeleteIIdentifiableWhenPolicyTrackChangesIsFalse()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            repo.Policy.TrackChanges = false; // prevents all change processing
            try
            {
                repo.Delete(new Patient() { Id = "#13:1" } as IIdentifiable);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CanNotDeleteTypeWhenPolicyTrackChangesIsFalse()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            repo.Policy.TrackChanges = false; // prevents all change processing
            try
            {
                repo.Delete(new Patient() { Id = "#13:1" });
            }
            catch (InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }
        #endregion

        #region Reading tests
        [TestMethod]
        public void CanNotEvaluateAssociationsNoPredicateExtensionOnUntrackedThing()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = new Patient();
            try
            {
                var result = patient.SourceOf<doctors>();
            }
            catch(InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CanNotEvaluateAssociationsNoPredicateExtensionOnInvalidRepo()
        {
            var patient = new Patient();
            using (var repo = new HealthRepository(new ODatabase(CONNECTION_NAME)))
            {
                repo.Attach(patient);
            }
            try
            {
                var result = patient.SourceOf<doctors>();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CanEvaluateAssociationsNoPredicateExtensionOnTrackedThing()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var source = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Source"; });
            var target = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Target"; });
            var docs = repo.Create<doctors>(d => { d.Source = source; d.Target = target; });

            repo.SaveChanges();
            repo.ClearChanges();

            // we want to make sure we're not pulling something from cache
            source = repo.Create<Patient>(p => p.Id = source.Id);

            var doctors = source.SourceOf<doctors>().First();
            Assert.IsTrue(doctors != null);
            Assert.IsTrue(doctors.Id == docs.Id);
        }

        [TestMethod]
        public void CanNotEvaluateAssociationsExtensionOnUntrackedThing()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = new Patient();
            try
            {
                var result = patient.SourceOf<doctors>(d => false);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CanNotEvaluateAssociationsExtensionOnInvalidRepo()
        {
            var patient = new Patient();
            using (var repo = new HealthRepository(new ODatabase(CONNECTION_NAME)))
            {
                repo.Attach(patient);
            }
            try
            {
                var result = patient.SourceOf<doctors>(d => false);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CanEvaluateAssociationsExtensionOnTrackedThing()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var source = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Source"; });
            var target = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Target"; });
            var docs = repo.Create<doctors>(d => { d.Source = source; d.Target = target; });

            repo.SaveChanges();
            repo.ClearChanges();

            // we want to make sure we're not pulling something from cache
            source = repo.Create<Patient>(p => p.Id = source.Id);

            var doctors = source.SourceOf<doctors>(doctorEdge => doctorEdge.Target == target).First();

            Assert.IsTrue(doctors != null);
            Assert.IsTrue(doctors.Id == docs.Id);
        }

        [TestMethod]
        public void CanNotEvaluateUntypedAssociationsExtensionOnUntrackedThing()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = new Patient();
            try
            {
                var result = patient.SourceOf();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CanNotEvaluateUntypedAssociationsExtensionOnInvalidRepo()
        {
            var patient = new Patient();
            using (var repo = new HealthRepository(new ODatabase(CONNECTION_NAME)))
            {
                repo.Attach(patient);
            }
            try
            {
                var result = patient.SourceOf();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void CanEvaluateUntypedAssociationsNoPredicateExtensionOnTrackedThing()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var source = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Source"; });
            var target = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Target"; });
            var docs = repo.Create<doctors>(d => { d.Source = source; d.Target = target; });

            repo.SaveChanges();
            repo.ClearChanges();

            // we want to make sure we're not pulling something from cache
            source = repo.Create<Patient>(p => p.Id = source.Id);

            var doctors = source.SourceOf().First();

            Assert.IsTrue(doctors != null);
            Assert.IsTrue(doctors.Id == docs.Id);
        }

        [TestMethod]
        public void CanEvaluateUntypedAssociationsWithPredicateExtensionOnTrackedThing()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var source = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Source"; });
            var target = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Target"; });
            var docs = repo.Create<doctors>(d => { d.Source = source; d.Target = target; });

            repo.SaveChanges();
            repo.ClearChanges();

            // we want to make sure we're not pulling something from cache
            source = repo.Create<Patient>(p => p.Id = source.Id);

            var doctors = source.SourceOf(a => a.Target == target).First();

            Assert.IsTrue(doctors != null);
            Assert.IsTrue(doctors.Id == docs.Id);
        }

        //[TestMethod]
        //public void CanEvaluateAssociationsWithNestedPredicateExtensionOnTrackedThing()
        //{
        //    var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
        //    var source = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Source"; });
        //    var target = repo.Create<Patient>(p => { p.Age = 5; p.Name = "Target"; });
        //    var docs = repo.Create<doctors>(d => { d.Source = source; d.Target = target; });

        //    repo.SaveChanges();
        //    repo.ClearChanges();

        //    // we want to make sure we're not pulling something from cache
        //    source = repo.Create<Patient>(p => p.Id = source.Id);

        //    // translates to:
        //    // select from doctors
        //    // where out = #13:6456 and in.outE()[@class='documents'][Type='Foo'][Size=3][in.@class='Document'][in.Name='ER'].size() > 0
        //    // and eval('COALESCE(PreferredPatient.out(), [])').size() > 0 -- complex property on edge hack

        //    //var doctors = source.SourceOf<doctors>
        //    //    (
        //    //        doctorE => doctorE.Target.SourceOf<documents>(documentE => documentE.Type == "Foo" && ((Document)documentE.Target).Name == "ER").Count() > 0
        //    //        && doctorE.PreferredPatient != null
        //    //    )
        //    //    .First();

        //    var doctors = repo.doctors.Where(
        //        doctorE =>
        //        doctorE.Source == source
        //        && repo.documents.Where(documentE => doctorE.Target == documentE.Source).Count() > 0)
        //        .First();

        //    //var doctors = repo.doctors.Where(
        //    //    doctorE => 
        //    //    doctorE.Source == source
        //    //    && repo.documents.Where(documentE => documentE.Source == doctorE.Target 
        //    //        && documentE.Type == "Foo" 
        //    //        && ((Document)documentE.Target).Name == "ER").Count() > 0
        //    //    && doctorE.PreferredPatient != null)
        //    //    .First();

        //    Assert.IsTrue(doctors != null);
        //    Assert.IsTrue(doctors.Id == docs.Id);
        //}
        #endregion

        #endregion

        #region Actors
        [TestMethod]
        public void CanCreateActorTypes()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var admin = repo.Create<Admin>((p) => { p.Name = "Pat"; });
            var user = repo.Create<User>(u => { u.Actor = admin; u.Name = "PatientUser"; });

            repo.SaveChanges();
            repo.ClearChanges();

            user = repo.Get(user);
            admin = repo.Get(admin);

            Assert.IsTrue(admin.Id != null, "Admin Id should not be null");
            Assert.IsTrue(user.Id != null, "User Id should not be null");
            Assert.IsTrue(Object.ReferenceEquals(user.Actor, admin), "Actor should be Admin");
        }
        #endregion

        #region Policy tests
        [TestMethod]
        public void CanDisableQueryCache()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            // diable query caching for this repo
            repo.Policy.UseQueryCache = false;

            repo.QueryCache.Clear();

            var patient = new Patient();

            using (var reader = repo.ExecuteReader("SELECT FROM Patient LIMIT 1"))
            {
                reader.Read();
                patient.Id = reader.GetString(reader.GetOrdinal("Id"));
                patient.Name = reader.GetString(reader.GetOrdinal("Name"));
                patient.Age = reader.GetInt32(reader.GetOrdinal("Age"));
            }

            var emptyPatient = new Patient() { Id = patient.Id };
            var getPatient = repo.Get(emptyPatient);

            Assert.IsTrue(patient.Id == getPatient.Id, "Ids are not equal");
            Assert.IsTrue(patient.Age == getPatient.Age, "Age is incorrect");
            Assert.IsTrue(patient.Name == getPatient.Name, "Name is incorrect");
            Assert.IsTrue(repo.QueryCache.Count == 0);
            //Assert.IsTrue(patient is IGraph<Patient>);
        }
        #endregion

        #region Reading/Querying tests
        [TestMethod]
        public void CanSupportScalarDataTypes()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var identifiable = new DataTypeTest()
            {
                ByteArray = new byte[] { 1, 2, 3, 4, 5 },
                IntArray = new int[] { 1, 2, 3, 4, 5, 6 },
                StringArray = new string[] { "Hello", "Cruel", "World!" },
                Boolean = true,
                Byte = byte.MaxValue,
                DateTime = new DateTime(2012, 12, 12, 12, 12, 12, 12),
                Decimal = decimal.MaxValue / 2M, // orient will round the max value up to a string which cannot be deserialized, it will also trim numeric precision when storing...
                Double = 1.2345e+23, // orient isn't handling exponential values correctly...
                Float = 1.2345e+6f, // orient isn't handling exponential values correctly...
                Integer = int.MaxValue,
                Long = long.MaxValue,
                SByte = sbyte.MaxValue,
                Short = short.MaxValue,
                UInt = uint.MaxValue,
                UShort = ushort.MaxValue,
                String = "Hello World!"
            };

            identifiable = repo.Attach(identifiable, TrackingState.ShouldSave);
            var dataTypeTest = repo.SaveChanges()
                .Get(identifiable);

            foreach (var prop in identifiable.GetType()
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => !p.Name.Equals("Id")))
            {
                Assert.IsTrue(PropsAreEqual(identifiable, dataTypeTest, prop), "Property values are not equal: " + prop.Name);
            }
        }

        [TestMethod]
        public void CanReadFastEnough(bool track, int limit, out int count)
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            repo.Policy.Limit = limit;
            repo.Policy.TrackChanges = track;
            var patients = repo.Patients.ToArray();
            count = patients.Count();
        }

        [TestMethod]
        public void CanEnumerateSubtypesFromBaseType()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<OrthopedicPatient>((p) => { p.Age = 3; p.Name = "Pat"; p.Joint = "Knee"; });
            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            var result = repo.Patients.ToArray();
            Assert.IsTrue(result.OfType<OrthopedicPatient>().Count() > 0);
        }

        [TestMethod]
        public void CanReturnProjectedResults()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var result = repo.Patients.Select(p => new { Name = p.Name, Foo = "fa" }).FirstOrDefault();
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Name != null);
            Assert.IsTrue(result.Foo == "fa");
        }

        [TestMethod]
        public void CanEvaluateTopLevelWherePredicateResults()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var name = "Ortho2";
            var age = 3;
            var state = "TX";
            var country = "USA";
            string countryId, addressId, patientId;

            #region Seed Data
            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Country SET Name='{0}' RETURN @this", country)))
            {
                reader.Read();
                countryId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Address SET Street1='{0}', Street2='{1}', City='{2}', State='{3}', Postal='{4}', Country={5} RETURN @this",
                "1 Main Street",
                null,
                "Tomball",
                state,
                "77355",
                countryId)))
            {
                reader.Read();
                addressId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Patient SET Name='{0}', Age={1}, Address={2} RETURN @this",
                name,
                age,
                addressId)))
            {
                reader.Read();
                patientId = reader.GetString(reader.GetOrdinal("Id"));
            }
            #endregion

            var result = repo.Patients.Where(p => p.Age == age && p.Name == name)
                .Select(p => new { Name = p.Name, Foo = "fa" })
                .FirstOrDefault();
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Name != null);
            Assert.IsTrue(result.Foo == "fa");
        }

        [TestMethod]
        public void CanEvaluateMultiSubLevelWherePredicateResults()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var age = 3;
            var state = "NY";
            var country = "USA";
            string countryId, addressId, patientId;

            #region Seed Data
            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Country SET Name='{0}' RETURN @this", country)))
            {
                reader.Read();
                countryId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Address SET Street1='{0}', Street2='{1}', City='{2}', State='{3}', Postal='{4}', Country={5} RETURN @this",
                "1 Main Street",
                null,
                "Tomball",
                state,
                "77355",
                countryId)))
            {
                reader.Read();
                addressId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Patient SET Name='{0}', Age={1}, Address={2} RETURN @this",
                "Patient",
                age,
                addressId)))
            {
                reader.Read();
                patientId = reader.GetString(reader.GetOrdinal("Id"));
            }
            #endregion

            var result = repo.Patients.Where(p =>
                    p.Age == age
                    && p.Address.State == state
                    && p.Address.Country.Name.Equals(country))
                .Select(p => new { Name = p.Name, Foo = "fa" })
                .FirstOrDefault();

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Name != null);
            Assert.IsTrue(result.Foo == "fa");
        }

        [TestMethod]
        public void CanDeferLoadMultipleSubLevelScalarComplexProperties()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var age = 3;
            var state = "TX";
            var country = "USA";
            string countryId, addressId, patientId;

            #region Seed Data
            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Country SET Name2='{0}' RETURN @this", country)))
            {
                reader.Read();
                countryId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Address SET Street1='{0}', Street2='{1}', City='{2}', State='{3}', Postal='{4}', Country={5} RETURN @this",
                "1 Main Street",
                null,
                "Tomball",
                state,
                "77355",
                countryId)))
            {
                reader.Read();
                addressId = reader.GetString(reader.GetOrdinal("Id"));
            }

            using (var reader = repo.ExecuteReader(
                string.Format("INSERT INTO Patient SET Name='{0}', Age={1}, Address={2} RETURN @this",
                "Patient",
                age,
                addressId)))
            {
                reader.Read();
                patientId = reader.GetString(reader.GetOrdinal("Id"));
            }
            #endregion

            var result = repo.Patients.Where(p =>
                    p.Age == age
                    && p.Address.State == state
                    && p.Address.Country.Name.Equals(country))
                .FirstOrDefault();
            var addressEnt = result.Address;
            var countryEnt = addressEnt.Country;

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Name != null);
            Assert.IsTrue(addressEnt != null);
            Assert.IsTrue(countryEnt != null);
        }

        [TestMethod]
        public void CanReadEnumerableIThingComplexProperties()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            string id;
            SetupPatientEnumerables(repo, out id);

            var patient = repo.ThingSet<Patient>().Single(p => p.Id == id);
            Assert.IsTrue(patient.IdentifiableEnumerable.Count() > 0);
            Assert.IsTrue(patient.PatientsArray.Count() > 0);
            Assert.IsTrue(patient.PatientsIEnumerable.Count() > 0);
            Assert.IsTrue(patient.PatientsIList.Count() > 0);
            Assert.IsTrue(patient.PatientsList.Count() > 0);
        }

        #endregion

        #region State Tracking Tests
        [TestMethod]
        public void CanReuseTypedProxyTypeForSaving()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat" + Environment.TickCount.ToString(); });
            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            var id = patient.Id;
            var age = 12;
            var name = patient.Name;
            patient.Age = age;
            repo.SaveChanges();

            patient = repo.Get(patient);

            Assert.IsTrue(patient.Id == id, "Ids are not equal");
            Assert.IsTrue(patient.Age == age, "Age is incorrect");
            Assert.IsTrue(patient.Name == name, "Name is incorrect");
            //Assert.IsTrue(patient is IGraph<Patient>);
        }

        [TestMethod]
        public void CanReuseIIdentiableProxyTypeForSaving()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat" + Environment.TickCount.ToString(); });
            repo.SaveChanges();
            var patient = repo.Get(identifiable);

            var id = patient.Id;
            var age = 12;
            var name = ((Patient)patient).Name;

            ((Patient)patient).Age = age;
            patient = repo.SaveChanges().Get(patient);

            Assert.IsTrue(patient.Id == id, "Ids are not equal");
            Assert.IsTrue(((Patient)patient).Age == age, "Age is incorrect");
            Assert.IsTrue(((Patient)patient).Name == name, "Name is incorrect");
            //Assert.IsTrue(patient is IGraph<Patient>);
        }


        [TestMethod]
        public void CanSuppressLocallyMarkedDeletes()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat"; });
            Assert.IsTrue(identifiable is IProxyIdentifiable);

            repo.SaveChanges(); // puts entity into Uknown/IsUnchanged state, allowing tracking manager to replace it when reading new values

            // mark the new item as Deleted, but don't commit the change
            repo.Delete(identifiable);

            repo.Policy.ReturnTrackedDeletes = false; // this is the default setting, to suppress tracked deletes in results
            try
            {
                var patient = repo.Get(identifiable); // since the record is marked deleted, this call should produce an exception
            }
            catch (InvalidOperationException)
            {
                return;
            }
            Assert.Fail("Get should throw InvalidOperationException");
        }

        [TestMethod]
        public void CanIncludeLocallyMarkedDeletes()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var identifiable = repo.Create<Patient>((p) => { p.Age = 3; p.Name = "Pat"; });
            Assert.IsTrue(identifiable is IProxyIdentifiable);

            repo.SaveChanges(); // puts entity into Uknown/IsUnchanged state, allowing tracking manager to replace it when reading new values

            // mark the new item as Deleted, but don't commit the change
            repo.Delete(identifiable);

            repo.Policy.ReturnTrackedDeletes = true; // this is an optional setting, to allow tracked deletes in results

            var patient = repo.Get(identifiable); // since the record is marked deleted, but policy is set to return deletes, we should get a record

            Assert.IsTrue(ReferenceEquals(patient, identifiable));
        }

        [TestMethod]
        public void CanUseICloneableOnPOCO()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var cloneWasCalled = false;
            var ortho = new OrthopedicPatient(() => cloneWasCalled = true, null);
            repo.Attach(ortho);
            Assert.IsTrue(cloneWasCalled);
        }

        [TestMethod]
        public void CanUseICopyableOnPOCO()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var copyWasCalled = false;
            var ortho = new OrthopedicPatient(null, () => copyWasCalled = true);
            repo.Attach(ortho);
            repo.SaveChanges(); // saving a new record will invoke the copy operation
            Assert.IsTrue(copyWasCalled);
        }

        [TestMethod]
        public void CanCreateTrackedItems()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = repo.Create<Patient>();
            var ortho = repo.Create<OrthopedicPatient>(o => { o.Name = "Ortho"; o.Age = 4; o.Joint = "Knee"; });
            Assert.IsTrue(patient != null);
            Assert.IsTrue(ortho != null);
            Assert.IsTrue(ortho.Name == "Ortho");
            Assert.IsTrue(ortho.Age == 4);
            Assert.IsTrue(ortho.Joint == "Knee");
            Assert.IsTrue(ortho is IProxyIdentifiable);
        }

        [TestMethod]
        public void CanCreateTrackedItemsFromFunc()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = repo.Create<Patient>();
            var ortho = repo.Create<OrthopedicPatient>(() => new OrthopedicPatient() { Name = "Ortho", Age = 4, Joint = "Knee" });
            Assert.IsTrue(patient != null);
            Assert.IsTrue(ortho != null);
            Assert.IsTrue(ortho.Name == "Ortho");
            Assert.IsTrue(ortho.Age == 4);
            Assert.IsTrue(ortho.Joint == "Knee");
            Assert.IsTrue(ortho is IProxyIdentifiable);
        }

        [TestMethod]
        public void CanEnumerateTrackedItems()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            repo.Create<Patient>();
            repo.Create<OrthopedicPatient>();
            Assert.IsTrue(repo.Count() == 2);
        }

        [TestMethod]
        public void CanAttachItemsWithCorrectState()
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////
            //              | Desired
            //===============================================================================================
            // Current      | New       | Modified  | Deleted           | Unknown   | Unchanged | NotTracked
            //-----------------------------------------------------------------------------------------------
            // Modified     |Modified   |Modified   |Deleted/NotTracked |Modified   |Unchanged  |NotTracked
            // Deleted      |Deleted    |Deleted    |Deleted            |Deleted    |Unchanged  |NotTracked
            // Unknown      |Unknown    |Modified   |Deleted            |Unknown    |Unchanged  |NotTracked
            // Unchanged    |Unchanged  |Modified   |Deleted            |Unknown    |Unchanged  |NotTracked
            // NotTracked   |NotTracked |Modified   |Deleted            |Unknown    |Unchanged  |NotTracked
            //===============================================================================================
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var patient = new Patient();

            #region empty tracking set
            patient = repo.Attach(patient, TrackingState.ShouldSave);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldSave);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.ShouldDelete);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldDelete);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.IsUnchanged);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsUnchanged);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.Unknown);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.IsNotTracked);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsNotTracked);
            Assert.IsTrue(repo.Any() == false);
            repo.ClearChanges();
            #endregion

            #region existing tracking entry - Modified state
            patient = repo.Attach(patient, TrackingState.ShouldSave); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldSave);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldSave);
            repo.ClearChanges();

            patient.Id = null;
            patient = repo.Attach(patient, TrackingState.ShouldSave); // create new entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldDelete);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsNotTracked);
            repo.ClearChanges();

            patient.Id = "#13:1";
            patient = repo.Attach(patient, TrackingState.ShouldSave); // create existing entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldDelete);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldDelete);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.ShouldSave); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsUnchanged);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsUnchanged);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.ShouldSave); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.Unknown);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldSave);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.ShouldSave); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsNotTracked);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsNotTracked);
            Assert.IsTrue(repo.Any() == false);
            repo.ClearChanges();
            #endregion

            #region existing tracking entry - Deleted state
            patient = repo.Attach(patient, TrackingState.ShouldDelete); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldSave);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldDelete);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.ShouldDelete); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldDelete);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldDelete);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.ShouldDelete); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsUnchanged);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsUnchanged);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.ShouldDelete); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.Unknown);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldDelete);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.ShouldDelete); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsNotTracked);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsNotTracked);
            Assert.IsTrue(repo.Any() == false);
            repo.ClearChanges();
            #endregion

            #region existing tracking entry - Unknown state
            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldSave);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldSave);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldDelete);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldDelete);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsUnchanged);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsUnchanged);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.Unknown);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.Unknown);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsNotTracked);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsNotTracked);
            Assert.IsTrue(repo.Any() == false);
            repo.ClearChanges();
            #endregion

            #region existing tracking entry - Unchanged state
            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldSave);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldSave);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldDelete);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldDelete);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsUnchanged);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsUnchanged);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.Unknown);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.Unknown);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.Unknown); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsNotTracked);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsNotTracked);
            Assert.IsTrue(repo.Any() == false);
            repo.ClearChanges();
            #endregion

            #region existing tracking entry - NotTracked state
            patient = repo.Attach(patient, TrackingState.IsNotTracked); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldSave);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldSave);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.IsNotTracked); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.ShouldDelete);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.ShouldDelete);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.IsNotTracked); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsUnchanged);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsUnchanged);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.IsNotTracked); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.Unknown);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.Unknown);
            repo.ClearChanges();

            patient = repo.Attach(patient, TrackingState.IsNotTracked); // create entry with Modified setting
            patient = repo.Attach(patient, TrackingState.IsNotTracked);
            Assert.IsTrue(repo.GetState(patient) == TrackingState.IsNotTracked);
            Assert.IsTrue(repo.Any() == false);
            repo.ClearChanges();
            #endregion
        }

        [TestMethod]
        public void CanGetDeleteItemFromTracking()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = repo.Create<Patient>();
            Assert.IsTrue(repo.Count() == 1);
            repo.Detach(patient);
            Assert.IsTrue(repo.Count() == 0);
        }

        [TestMethod]
        public void CanAcceptChanges()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            repo.Create<Patient>();
            repo.Create<Patient>();
            repo.Create<Patient>();
            repo.Create<Patient>();
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.ShouldSave) == 4);
            repo.AcceptChanges();
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.ShouldSave) == 0);
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.Unknown) == 4);
        }

        [TestMethod]
        public void CanRemoveNewItemsByTypeWhenDeleting()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            repo.Delete(repo.Create<Patient>());
            repo.Delete(repo.Create<Patient>());
            repo.Delete(repo.Create<Patient>());
            repo.Delete(repo.Create<Patient>());

            Assert.IsTrue(repo.Count() == 0);
        }

        [TestMethod]
        public void CanRemoveNewItemsByInterfaceWhenDeleting()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            repo.Delete((IIdentifiable)repo.Create<Patient>());
            repo.Delete((IIdentifiable)repo.Create<Patient>());
            repo.Delete((IIdentifiable)repo.Create<Patient>());
            repo.Delete((IIdentifiable)repo.Create<Patient>());

            Assert.IsTrue(repo.Count() == 0);
        }

        [TestMethod]
        public void CanUpdateNewItemsByTypeWhenDeleting()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            repo.Delete(repo.Create<Patient>(p => p.Id = "#13:1"));
            repo.Delete(repo.Create<Patient>(p => p.Id = "#13:1"));
            repo.Delete(repo.Create<Patient>(p => p.Id = "#13:1"));
            repo.Delete(repo.Create<Patient>(p => p.Id = "#13:1"));

            Assert.IsTrue(repo.Count(t => t.State == TrackingState.ShouldDelete) == 1);
        }

        [TestMethod]
        public void CanUpdateNewItemsByInterfaceWhenDeleting()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            repo.Delete((IIdentifiable)repo.Create<Patient>(p => p.Id = "#13:1"));
            repo.Delete((IIdentifiable)repo.Create<Patient>(p => p.Id = "#13:1"));
            repo.Delete((IIdentifiable)repo.Create<Patient>(p => p.Id = "#13:1"));
            repo.Delete((IIdentifiable)repo.Create<Patient>(p => p.Id = "#13:1"));

            Assert.IsTrue(repo.Count(t => t.State == TrackingState.ShouldDelete) == 1);
        }

        [TestMethod]
        public void CanUpdateNewItemsByIdWhenDeleting()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            repo.Delete(repo.Create<Patient>(p => p.Id = "#13:1").Id);
            repo.Delete(repo.Create<Patient>(p => p.Id = "#13:1").Id);
            repo.Delete(repo.Create<Patient>(p => p.Id = "#13:1").Id);
            repo.Delete(repo.Create<Patient>(p => p.Id = "#13:1").Id);

            Assert.IsTrue(repo.Count(t => t.State == TrackingState.ShouldDelete) == 1);
        }

        [TestMethod]
        public void CanDetectINotifyPropertyChangedChanges()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            var notify = repo.Create<Notify>();
            repo.AcceptChanges();
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.Unknown) == 1);
            notify.Name = "new name";
            notify.OtherNames2.Add("new name");
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.Unknown) == 0);
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.ShouldSave) == 1);
        }

        [TestMethod]
        public void CanDetectINotifyCollectionChangedChanges()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));

            repo.Policy.LookForINotifyCollectionChanged = true; // this is disabled by default

            var notify = repo.Create<Notify>();
            repo.AcceptChanges();
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.Unknown) == 1);
            notify.OtherNames.Add("Another name");
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.Unknown) == 0);
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.ShouldSave) == 1);
        }

        [TestMethod]
        public void CanNotDetectINotifyCollectionChangedChangesByDefault()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var notify = repo.Create<Notify>();
            repo.AcceptChanges();
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.Unknown) == 1);
            notify.OtherNames.Add("Another name");
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.Unknown) == 1);
            Assert.IsTrue(repo.Count(t => t.State == TrackingState.ShouldSave) == 0);
        }

        [TestMethod]
        public void CanDefaultCloneTrackedIdentifiable()
        {
            var patient = new Patient() { Id = "#13:1", Name = "Foo", Age = 4, PatientsArray = new Patient[10] };
            patient.PatientsArray[0] = new Patient() { Name = "Array" };
            patient.PatientsIEnumerable = patient.PatientsArray.ToList().AsEnumerable();
            var tracked = new UnitOfWork.TrackedIdentifiable<Patient>(patient, null, new IdentifiableCopier(null));
            Assert.IsTrue(tracked.Identifiable == patient);
            Assert.IsTrue(tracked.Original != patient);
            Assert.IsTrue(tracked.Original.Id == patient.Id);
            Assert.IsTrue(tracked.Original.Name == patient.Name);
            Assert.IsTrue(tracked.Original.Age == patient.Age);
            Assert.IsTrue(tracked.Original.PatientsArray != patient.PatientsArray);
            Assert.IsTrue(tracked.Original.PatientsArray[0] != patient.PatientsArray[0]);
            Assert.IsTrue(tracked.Original.PatientsIEnumerable != patient.PatientsIEnumerable);
            Assert.IsTrue(tracked.Original.PatientsIEnumerable.First() != patient.PatientsIEnumerable.First());
        }

        [TestMethod]
        public void CanManuallyCalculateTrackingState()
        {
            var patient = new Patient() { Id = "#13:1", Name = "Foo", Age = 4, PatientsArray = new Patient[10] };
            patient.PatientsArray[0] = new Patient() { Name = "Array" };
            patient.PatientsIEnumerable = patient.PatientsArray.ToList().AsEnumerable();
            patient.Address = new Address() { Id = "#12:1", City = "Foo" };
            var tracked = new UnitOfWork.TrackedIdentifiable<Patient>(patient, null, new IdentifiableCopier(null));
            Assert.IsTrue(tracked.Identifiable == patient);
            Assert.IsTrue(tracked.Original != patient);
            Assert.IsTrue(tracked.State == TrackingState.Unknown);
            tracked.CalculateState();
            Assert.IsTrue(tracked.State == TrackingState.IsUnchanged);

            tracked.State = TrackingState.Unknown;
            patient.Age = 7;
            tracked.CalculateState();
            Assert.IsTrue(tracked.State == TrackingState.ShouldSave);

            patient.Age = 4;
            tracked.CalculateState();
            Assert.IsTrue(tracked.State == TrackingState.ShouldSave);

            tracked.CalculateState(true);
            Assert.IsTrue(tracked.State == TrackingState.IsUnchanged);

            tracked.ResetState();
            Assert.IsTrue(tracked.State == TrackingState.Unknown);

            patient.Age = 8;
            tracked.CalculateState();
            Assert.IsTrue(tracked.State == TrackingState.ShouldSave);

            tracked.ResetState();
            patient.PatientsArray[0].Id = "#13:5";
            tracked.CalculateState();
            Assert.IsTrue(tracked.State == TrackingState.ShouldSave);

            tracked.State = TrackingState.ShouldDelete;
            patient.Age = 12;
            Assert.IsTrue(tracked.State == TrackingState.ShouldDelete);

            tracked.ResetState();
            patient.Address.City = "Fass";
            tracked.CalculateState();
            Assert.IsTrue(tracked.State == TrackingState.IsUnchanged);

            tracked.ResetState();
            patient.Address.Id = "#12:2";
            tracked.CalculateState();
            Assert.IsTrue(tracked.State == TrackingState.ShouldSave);
        }

        [TestMethod]
        public void CanCascadeDetachLoadedItems()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var patient = repo.Create<Patient>();
            patient.Sibling = repo.Create<Patient>();
            patient.PatientsArray = new Patient[] { repo.Create<Patient>(), new Patient() };
            Assert.IsTrue(repo.Count() == 4);
            repo.Detach(patient, true);
            Assert.IsTrue(repo.Count() == 0);
        }
        #endregion

        #region Schema tests
        [TestMethod]
        public void CanReadSchema()
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            Assert.IsTrue(repo.SchemaBuilder != null);
            Assert.IsTrue(repo.SchemaBuilder.IsInitialized);
            Assert.IsTrue(repo.SchemaBuilder.Schema != null);
            Assert.IsTrue(repo.SchemaBuilder.Schema.Classes.Count() > 0);
        }
        #endregion

        #region Helpers
        public T Get<T>(string id)
        {
            var repo = new HealthRepository(new ODatabase(CONNECTION_NAME));
            var result = (T)repo.Get(id);
            return result;
        }

        private void SetupPatientEnumerables(HealthRepository repo, out string id)
        {
            id = repo.ExecuteEnumerable<Patient>("INSERT INTO Patient (Name, Age) VALUES('Test', 4) RETURN @this").First().Id;
            var patientId2 = repo.ExecuteEnumerable<Patient>("INSERT INTO Patient (Name, Age) VALUES('Child', 4) RETURN @this").First().Id;
            var patientId3 = repo.ExecuteEnumerable<Patient>("INSERT INTO Patient (Name, Age) VALUES('Child', 4) RETURN @this").First().Id;
            repo.ExecuteStatement("UPDATE " + id + " Add IdentifiableEnumerable = [" + patientId2 + ", " + patientId3 + "]");
            repo.ExecuteStatement("UPDATE " + id + " SET PatientsIEnumerable = IdentifiableEnumerable, PatientsIList = IdentifiableEnumerable, PatientsIList = IdentifiableEnumerable, PatientsList = IdentifiableEnumerable, PatientsArray = IdentifiableEnumerable");
        }
        #endregion

        private string GetFirstId(IRepository repo, string className)
        {
            using (var reader = repo.ExecuteReader("SELECT FROM " + className + " LIMIT 1"))
            {
                reader.Read();
                return reader.GetString(reader.GetOrdinal("Id"));
            }
        }

        private bool PropsAreEqual(DataTypeTest identifiable, DataTypeTest dataTypeTest, PropertyInfo prop)
        {
            if (prop.PropertyType.IsArray)
            {
                var iArray = (Array)prop.GetValue(identifiable);
                var rArray = (Array)prop.GetValue(dataTypeTest);
                if (rArray.Rank != iArray.Rank
                    || rArray.Length != iArray.Length)
                {
                    return false;
                }
                for (int i = 0; i < iArray.Length; i++)
                {
                    if (!iArray.GetValue(i).Equals(rArray.GetValue(i))) return false;
                }
                return true;
            }
            else
            {
                if (prop.Name == "DateTime")
                {
                    // orient is only accurate to seconds on date time types, so ignore milliseconds
                    return identifiable.DateTime.Date.Equals(dataTypeTest.DateTime.Date)
                        && identifiable.DateTime.Hour.Equals(dataTypeTest.DateTime.Hour)
                        && identifiable.DateTime.Minute.Equals(dataTypeTest.DateTime.Minute)
                        && identifiable.DateTime.Second.Equals(dataTypeTest.DateTime.Second);
                }
                else if (prop.Name == "Decimal")
                {
                    return Math.Abs(1 - identifiable.Decimal / dataTypeTest.Decimal) < 0.0000001M;
                }
                else return prop.GetValue(identifiable).Equals(prop.GetValue(dataTypeTest));
            }
        }
        #endregion

        static bool _isSetup = false;
        [TestInitialize]
        public void Setup()
        {
            if (!_isSetup)
            {
                var databaseName = "Schema-Tests";
                //DbRunner.StartOrientDb(@"C:\OrientDb", @"C:\Program Files\Java\jre1.8.0_31");
                OClient.CreateDatabasePool(
                    "127.0.0.1",
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

        [TestCleanup]
        public void TearDown()
        {
            //DbRunner.StopOrientDb();
        }
    }
}
