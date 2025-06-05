using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TripApi.Data;
using TripApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TripApi.Data;
using TripApi.Data.Entities;
using TripApi.DTOs;

namespace TripApi.Services
{
    public class TripService : ITripService
    {
        private readonly TripDbContext _context;

        public TripService(TripDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a paginated list of trips (sorted by DateFrom descending),
        /// including related countries and clients.
        /// </summary>
        public async Task<PaginatedResult<TripDto>> GetTripsAsync(int pageNum, int pageSize)
        {
            if (pageNum < 1) pageNum = 1;
            if (pageSize < 1) pageSize = 10;

            // 1. Count total number of trips
            var totalCount = await _context.Trips.CountAsync();

            // 2. Calculate total pages
            var allPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 3. Query trips with related data, ordered by DateFrom descending
            var trips = await _context.Trips
                .Include(t => t.IdCountries)           // (1) Include direct navigation to Country
                .Include(t => t.Client_Trips)          // (2) Include the join table for clients
                    .ThenInclude(ct => ct.IdClientNavigation) // then load the Client entity
                .OrderByDescending(t => t.DateFrom)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. Map to DTOs
            var tripDtos = trips.Select(t => new TripDto
            {
                IdTrip = t.IdTrip,
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,

                // Map countries from t.IdCountries
                Countries = t.IdCountries
                    .Select(c => new CountryInfo
                    {
                        Name = c.Name
                    })
                    .ToList(),

                // Map clients from t.Client_Trips
                Clients = t.Client_Trips
                    .Select(ct => new ClientInfo
                    {
                        FirstName = ct.IdClientNavigation.FirstName,
                        LastName = ct.IdClientNavigation.LastName
                    })
                    .ToList()
            }).ToList();

            // 5. Return paginated result
            return new PaginatedResult<TripDto>
            {
                PageNum = pageNum,
                PageSize = pageSize,
                AllPages = allPages,
                Items = tripDtos
            };
        }
        
        /// Adds a new client (or uses an existing client if the Pesel already exists) to a trip.
        public async Task<string> AddClientToTripAsync(int idTrip, CreateClientForTripDto dto)
        {
            // 1. Check if the trip exists
            var trip = await _context.Trips
                .Include(t => t.Client_Trips)
                .FirstOrDefaultAsync(t => t.IdTrip == idTrip);
            if (trip == null)
            {
                return $"Trip with Id = {idTrip} does not exist.";
            }

            // 2. Check that the trip's DateFrom is in the future
            if (trip.DateFrom <= DateTime.UtcNow)
            {
                return "Cannot register for a trip that has already started or finished.";
            }

            // 3. Find an existing client by Pesel
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);
            if (existingClient != null)
            {
                // If the client exists, check if they are already registered for this trip
                bool alreadyInTrip = await _context.Client_Trips
                    .AnyAsync(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);
                if (alreadyInTrip)
                {
                    return $"Client with Pesel = {dto.Pesel} is already registered for this trip.";
                }
            }

            // If the client does not exist, create a new one
            Client clientToUse = existingClient;
            if (existingClient == null)
            {
                clientToUse = new Client
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Telephone = dto.Telephone,
                    Pesel = dto.Pesel
                };
                _context.Clients.Add(clientToUse);
                // SaveChanges will assign the new Id automatically
            }

            // 4. (Optional) Check if the number of registered clients >= MaxPeople
            // var registeredCount = await _context.Client_Trips.CountAsync(ct => ct.IdTrip == idTrip);
            // if (registeredCount >= trip.MaxPeople)
            // {
            //     return "This trip has reached the maximum number of participants.";
            // }

            // 5. Create a new Client_Trip record
            var clientTrip = new Client_Trip
            {
                IdTrip = idTrip,
                IdClient = clientToUse.IdClient, // for a new client, EF will assign after SaveChanges
                RegisteredAt = DateTime.UtcNow,
                PaymentDate = dto.PaymentDate
            };
            _context.Client_Trips.Add(clientTrip);

            // 6. Save all changes
            await _context.SaveChangesAsync();
            return null; // null indicates success
        }
    }
}
