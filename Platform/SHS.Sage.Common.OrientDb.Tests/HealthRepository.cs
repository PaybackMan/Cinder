using Orient.Client;
using SHS.Sage.OrientDb.Tests.Associations;
using SHS.Sage.OrientDb.Tests.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHS.Sage.Linq;
using SHS.Sage.Linq.Language;
using SHS.Sage.Mapping;
using SHS.Sage.Linq.Mapping;

namespace SHS.Sage.OrientDb.Tests
{
    public class HealthRepository : ORepository
    {
        public HealthRepository(ODatabase connection) : base(connection) { }

        public IThingSet<Patient> Patients { get { return base.ThingSet<Patient>(); } }

        public IThingSet<Doctor> Doctors { get { return base.ThingSet<Doctor>(); } }

        public IThingSet<Document> Documents { get { return base.ThingSet<Document>(); } }

        public IAssociationSet<doctors> doctors { get { return base.AssociationSet<doctors>(); } }

        public IAssociationSet<documents> documents { get { return base.AssociationSet<documents>(); } }

        public IThingSet<User> Users {  get { return base.ThingSet<User>(); } }

        protected override void OnRegisterIdentifiables(IFluentMapper mapper)
        {
            mapper.MapEntity<OrthopedicPatient>().DefaultMappings()
                  .MapEntity<Address>().DefaultMappings()
                  .MapEntity<Admin>().DefaultMappings()
                  .MapEntity<Country>().MapProperty(country => country.Name).StorageName(() => "Name2")
                                       .MapProperty(country => country.Id).Entity.Mapper
                  .MapEntity<DataTypeTest>().DefaultMappings();
        }
    }
}
