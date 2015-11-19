using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Core.Domain.Administrative
{
///================================================================================================================================
/// <summary>
/// A time period defined by a start and end date/time. 
/// A period specifies a range of times. The context of use will specify whether the entire range applies 
/// (e.g. "the patient was an inpatient of the hospital for this time range") or one value from the period 
/// applies (e.g. "give to the patient between 2 and 4 pm"). 
/// </summary>
///================================================================================================================================
    public class Period  
    {
///===============================================================================================================================
/// <summary>
/// Default Ctor.. 
/// </summary>
///===============================================================================================================================
        public Period()
        { }
///================================================================================================================================
/// <summary>
/// Starting time with inclusive boundary 
/// </summary>
///================================================================================================================================
        public DateTime Start { get; set; }
///================================================================================================================================
/// <summary>
/// End time with inclusive boundary, if not ongoing 
/// </summary>
///================================================================================================================================
        public DateTime End { get; set; }

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
