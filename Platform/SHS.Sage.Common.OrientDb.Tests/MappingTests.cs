using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orient.Client;
using SHS.Sage.Common.Linq;
using SHS.Sage.Common.OrientDb.Tests.Things;

namespace SHS.Sage.Common.OrientDb.Tests
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        public void CanExplicityMapSimpleThingProperties()
        {
            var repo = new ExplicitMappingRepo(null);
            var builder = repo.MappingRegistrar.GetBuilder(typeof(Patient));
            var mappedProps = builder.GetProperties(typeof(Patient));
            Assert.IsTrue(mappedProps.Count() == 2);
            Assert.IsTrue(!mappedProps.Any(p => p.Name.Equals("Id")));
            Assert.IsTrue(mappedProps.Any(p => p.Name.Equals("NAME")));
            Assert.IsTrue(mappedProps.Any(p => p.Name.Equals("Age")));
        }

        [TestMethod]
        public void CanImplicitlyMapSimpleThingProperties()
        {
            var repo = new ImplicitMappingRepo(null);
            var builder = repo.MappingRegistrar.GetBuilder(typeof(Patient));
            var mappedProps = builder.GetProperties(typeof(Patient));
            Assert.IsTrue(mappedProps.Count() == 2);
            Assert.IsTrue(!mappedProps.Any(p => p.Name.Equals("NAME")));
            Assert.IsTrue(mappedProps.Any(p => p.Name.Equals("Age")));
            Assert.IsTrue(!mappedProps.Any(p => p.Name.Equals("Id")));
        }

        [TestInitialize]
        public void Setup()
        {
            OGraphTypeBuilder.Clear();
        }
    }

    public class ExplicitMappingRepo : ORepository
    {
        public ExplicitMappingRepo(ODatabase db) : base(db)
        {
            this.MappingRegistrar.Clear();
        }

        protected override void OnModelCreating(IRegisterGraphMappings registry)
        {
            registry.Add(new Mapping(true));
            base.OnModelCreating(registry);
        }
    }

    public class ImplicitMappingRepo : ORepository
    {
        public ImplicitMappingRepo(ODatabase db) : base(db)
        {
            this.MappingRegistrar.Clear();
        }

        protected override void OnModelCreating(IRegisterGraphMappings registry)
        {
            registry.Add(new Mapping(false));
            base.OnModelCreating(registry);
        }
    }
}
