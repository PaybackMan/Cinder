using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SHS.Sage.Common.Linq;
using Orient.Client;
using SHS.Sage.Common.OrientDb.Tests.Things;
using SHS.Sage.Common.OrientDb.Tests.Associations;

namespace SHS.Sage.Common.OrientDb.Tests
{
    [TestClass]
    public class ProxyTypeTests : ORepository
    {
        [TestMethod]
        public void CanBuildProxyTypes()
        {
            this.GraphTypeBuilder.BuildTypes(this);
            Assert.IsTrue(this.GraphTypeBuilder.BuiltTypes.Any(bt => bt.BaseType.Equals(typeof(Patient))));
            Assert.IsTrue(this.GraphTypeBuilder.BuiltTypes.Any(bt => bt.BaseType.Equals(typeof(Doctor))));
            Assert.IsTrue(this.GraphTypeBuilder.BuiltTypes.Any(bt => bt.BaseType.Equals(typeof(doctors))));
        }

        public ProxyTypeTests() : base(null, false)
        {

        }

        public IThingSet<Patient> Patients { get { return base.ThingSet<Patient>(); } }

        public IThingSet<Doctor> Doctors { get { return base.ThingSet<Doctor>(); } }

        public IAssociationSet<doctors> doctors { get { return base.AssociationSet<doctors>(); } }

        protected override void OnModelCreating(IRegisterGraphMappings registry)
        {
            // hook this to add any specialized type mappings
            // registry.Add(new Mapping());
            base.OnModelCreating(registry);
        }
    }
}
