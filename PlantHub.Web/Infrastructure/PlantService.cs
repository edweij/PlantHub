using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;

namespace PlantHub.Web.Infrastructure
{
    public interface IPlantService
    {
        Task<Plant> AddAsync(Plant plant, CancellationToken ct = default);
        Task<IReadOnlyList<Plant>> ListAsync(CancellationToken ct = default);
    }

    public class PlantService : IPlantService
    {
        private readonly PlantHubDbContext _db;

        public PlantService(PlantHubDbContext db) => _db = db;

        public async Task<Plant> AddAsync(Plant plant, CancellationToken ct = default)
        {
            _db.Plants.Add(plant);
            await _db.SaveChangesAsync(ct);
            return plant;
        }

        public async Task<IReadOnlyList<Plant>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Plants
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync(ct);
        }
    }
}
