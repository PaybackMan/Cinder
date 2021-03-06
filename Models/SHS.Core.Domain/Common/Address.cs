﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;

namespace Cinder.Core.Domain.Common
{
    public enum Use
    {
        Home,
        Work,
        Temp,
        Old
    }

///===============================================================================================================================
/// <summary>
/// A postal address. There is a variety of postal address formats defined around the world. Postal addresses are often also used 
/// to record a location that can be visited to find a patient or person. 
/// </summary>
///===============================================================================================================================
    public class Address  
    {
        private string _fullAddress;
        private string _line;
        private string _city;
        private string _state;
        private string _postalCode;
        private string _country;
        private Period _period;
        private Use _use = Use.Work;

///===============================================================================================================================
/// <summary>
/// Default Ctor.. 
/// </summary>
///===============================================================================================================================
        public Address()
        { }
///===============================================================================================================================
/// <summary>
/// Text representation of the address 
/// </summary>
///===============================================================================================================================
        public Use Use
        {
            get { return _use; }
            set { _use = value; }
        }
///===============================================================================================================================
/// <summary>
/// Text representation of the address 
/// </summary>
///===============================================================================================================================
        public string FullAddress
        {
            get { return _fullAddress; }
            set { _fullAddress = value; }
        }
///===============================================================================================================================
/// <summary>
/// 
/// </summary>
///===============================================================================================================================
        public string CityState
        {
            get { return this.City + ", " + this.State; }
        }
///===============================================================================================================================
/// <summary>
/// Street name, number, direction & P.O. Box etc -->
/// </summary>
///===============================================================================================================================
        public string Line
        {
            get { return _line; }
            set { _line = value; }
        }
///===============================================================================================================================
/// <summary>
/// Name of city, town etc
/// </summary>
///===============================================================================================================================
        public string City
        {
            get { return _city; }
            set { _city = value; }
        }
///===============================================================================================================================
/// <summary>
/// Sub-unit of country (abreviations ok)
/// </summary>
///===============================================================================================================================
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }
///===============================================================================================================================
/// <summary>
/// Postal code for area
/// </summary>
///===============================================================================================================================
        public string PostalCode
        {
            get { return _postalCode; }
            set { _postalCode = value; }
        }
///===============================================================================================================================
/// <summary>
/// Country (can be ISO 3166 3 letter code)
/// </summary>
///===============================================================================================================================
        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }
//=================================================================================================================
/// <summary>
/// Time period when address was/is in use
/// </summary>
//=================================================================================================================
        public Period Period
        {
            get { return _period; }
            set { _period = value; }
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
