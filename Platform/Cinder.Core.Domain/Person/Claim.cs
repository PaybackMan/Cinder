using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Core.Domain.Person
{
    public class Claim
    {
        private string Value { get; set; }
        private string ValueType { get; set; }
        private string ClaimType { get; set; }
        private string Issuer { get; set; }
        private string OriginalIssuer { get; set; }
        private string Type { get; set; }

        public Claim()
        {
        }

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
    }
}
