using System.Threading.Tasks;
using TripApi.DTOs;

namespace TripApi.Services
{
    public interface ITripService
    {
        Task<PaginatedResult<TripDto>> GetTripsAsync(int pageNum, int pageSize);
        
        Task<string> AddClientToTripAsync(int idTrip, CreateClientForTripDto dto);
    }
}