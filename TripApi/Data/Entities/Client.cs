using System;
using System.Collections.Generic;

namespace TripApi.Data.Entities;

public partial class Client
{
    public int IdClient { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Telephone { get; set; } = null!;

    public string Pesel { get; set; } = null!;

    public virtual ICollection<Client_Trip> Client_Trips { get; set; } = new List<Client_Trip>();
}
