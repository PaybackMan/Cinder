using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Core.Domain.Administrative
{
    public class Position 
    {
        public string Id { get; set; }
        ///================================================================================================================================
        /// <summary>
        /// Longitude. The value domain and the interpretation are the same as for the text of the longitude element in KML
        /// </summary>
        ///================================================================================================================================
        public decimal Longitude { get; set; }
        ///================================================================================================================================
        /// <summary>
        /// titude. The value domain and the interpretation are the same as for the text of the altitude element in KML
        /// </summary>
        ///================================================================================================================================
        public decimal Altitude { get; set; }
        ///================================================================================================================================
        /// <summary>
        /// Latitude. The value domain and the interpretation are the same as for the text of the latitude element in KML
        /// </summary>
        ///================================================================================================================================
        public decimal Latitude { get; set; }
    }
}
