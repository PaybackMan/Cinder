using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.OrientDb.Tests.Things
{
    public class Patient : IActor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public virtual Address Address { get; set; }
        public virtual IEnumerable<IIdentifiable> IdentifiableEnumerable { get; set; }
        public virtual Patient[] PatientsArray { get; set; }
        public virtual IEnumerable<Patient> PatientsIEnumerable { get; set; }
        public virtual IList<Patient> PatientsIList { get; set; }
        public virtual List<Patient> PatientsList { get; set; }
        public virtual Patient Sibling { get; set; }
    }
}
