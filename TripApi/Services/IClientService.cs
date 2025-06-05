using System.Threading.Tasks;
using TripApi.DTOs;

namespace TripApi.Services
{
    public interface IClientService
    {
        Task<DeleteClientResultDto> DeleteClientAsync(int idClient);
    }
}