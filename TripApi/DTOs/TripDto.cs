using System;
using System.Collections.Generic;

namespace TripApi.DTOs
{
    public class TripDto
    {
        public int IdTrip { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int MaxPeople { get; set; }
        
        public List<CountryInfo> Countries { get; set; }
        
        public List<ClientInfo> Clients { get; set; }
    }

    public class CountryInfo
    {
        public string Name { get; set; }
    }

    public class ClientInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}