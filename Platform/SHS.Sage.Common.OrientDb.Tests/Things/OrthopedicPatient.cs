using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.OrientDb.Tests.Things
{
    public class OrthopedicPatient : Patient, ICloneable<OrthopedicPatient>, ICopyable<OrthopedicPatient>
    {
        private Action _cloneCallback;
        private Action _copyCallback;

        public OrthopedicPatient() { }

        public OrthopedicPatient(Action cloneCallback, Action copyCallback)
        {
            this._cloneCallback = cloneCallback;
            this._copyCallback = copyCallback;
        }

        public string Joint { get; set; }

        public OrthopedicPatient Clone()
        {
            var clone = new OrthopedicPatient()
            {
                Id = this.Id,
                Address = this.Address,
                Age = this.Age,
                IdentifiableEnumerable = this.IdentifiableEnumerable,
                Joint = this.Joint,
                Name = this.Name,
                PatientsArray = this.PatientsArray,
                PatientsIEnumerable = this.PatientsIEnumerable,
                PatientsIList = this.PatientsIList,
                PatientsList = this.PatientsList,
                _copyCallback = this._copyCallback,
                _cloneCallback = this._cloneCallback
            };
            if (_cloneCallback != null) _cloneCallback();
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public void CopyTo(OrthopedicPatient destination)
        {
            Copy(this, destination);
        }

        public void CopyTo(object destination)
        {
            CopyTo(destination as OrthopedicPatient);
        }

        public void CopyFrom(OrthopedicPatient source)
        {
            Copy(source, this);
        }

        public void CopyFrom(object source)
        {
            CopyFrom(source as OrthopedicPatient);
        }

        private void Copy(OrthopedicPatient source, OrthopedicPatient destination)
        {
            if (destination != null && source != null)
            {
                destination.Id = source.Id;
                destination.Address = source.Address;
                destination.Age = source.Age;
                destination.IdentifiableEnumerable = source.IdentifiableEnumerable;
                destination.Joint = source.Joint;
                destination.Name = source.Name;
                destination.PatientsArray = source.PatientsArray;
                destination.PatientsIEnumerable = source.PatientsIEnumerable;
                destination.PatientsIList = source.PatientsIList;
                destination.PatientsList = source.PatientsList;
                destination._cloneCallback = source._cloneCallback;
                destination._copyCallback = source._copyCallback;
            }
            if (_copyCallback != null) _copyCallback();
        }
    }
}
