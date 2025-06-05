using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TripApi.Data;
using TripApi.DTOs;
using TripApi.Data.Entities;

namespace TripApi.Services
{
    public class ClientService : IClientService
    {
        private readonly TripDbContext _context;

        public ClientService(TripDbContext context)
        {
            _context = context;
        }

        public async Task<DeleteClientResultDto> DeleteClientAsync(int idClient)
        {
            // 1. Retrieve the client including their Client_Trips
            var client = await _context.Clients
                .Include(c => c.Client_Trips)
                .FirstOrDefaultAsync(c => c.IdClient == idClient);
            if (client == null)
            {
                return new DeleteClientResultDto
                {
                    Success = false,
                    Message = $"Client with Id = {idClient} does not exist."
                };
            }

            // 2. If the client is participating in at least one trip
            if (client.Client_Trips != null && client.Client_Trips.Count > 0)
            {
                return new DeleteClientResultDto
                {
                    Success = false,
                    Message = "Cannot delete: this client is registered for at least one trip."
                };
            }

            // 3. No trips found, safe to delete
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return new DeleteClientResultDto
            {
                Success = true,
                Message = "Client deleted successfully."
            };
        }
    }
}