using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Core.Domain.Common
{
    public class CodeableConcept  
    {
        private string _name;

        private string _id;

        public CodeableConcept()
        {
        }


        public CodeableConcept(string name)
        {
            _name = name;
        }


        ///===============================================================================================================================
        /// <summary>
        /// Unique identifier of the Entity.. 
        /// </summary>
        ///===============================================================================================================================
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                //OnPropertyChanged(nameof(Id));
            }
        }

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
    }
}
