using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;

namespace PlantHub.Web.Infrastructure
{
    public interface IPlantService
    {
        Task DeleteAsync(Plant plant, CancellationToken ct = default);
        Task<Plant> AddAsync(Plant plant, CancellationToken ct = default);
        Task<IReadOnlyList<Plant>> ListAsync(CancellationToken ct = default);
        Task<Plant> UpdateAsync(Plant plant, CancellationToken ct = default);
        Task MarkWateredAsync(int plantId, CancellationToken ct = default);
        Task MarkWateredGroupAsync(int wateringGroupId, CancellationToken ct = default);
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

        public Task DeleteAsync(Plant plant, CancellationToken ct = default)
        {
            // Soft delete
            //plant.IsActive = false;
            //_db.Plants.Update(plant);
            _db.Plants.Remove(plant);
            return _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<Plant>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Plants
                .Where(p => p.IsActive)
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync(ct);
        }

        public async Task<Plant> UpdateAsync(Plant plant, CancellationToken ct = default)
        {
            _db.Plants.Update(plant);
            await _db.SaveChangesAsync(ct);
            return plant;
        }

        public async Task MarkWateredAsync(int plantId, CancellationToken ct = default)
        {
            var exists = await _db.Plants
                .AnyAsync(p => p.Id == plantId);

            if (!exists)
                throw new InvalidOperationException("Plant not found");

            var evt = new WateringEvent
            {
                PlantId = plantId,
                TimestampUtc = DateTime.UtcNow
            };

            _db.WateringEvents.Add(evt);
            await _db.SaveChangesAsync();
        }

        public async Task MarkWateredGroupAsync(int wateringGroupId, CancellationToken ct = default)
        {
            var plantIds = await _db.Plants
        .Where(p => p.WateringGroupId == wateringGroupId)
        .Select(p => p.Id)
        .ToListAsync();

            if (plantIds.Count == 0)
                return;

            var now = DateTime.UtcNow;

            // 🔒 Spärr: om något i gruppen vattnats nyligen
            var last = await _db.WateringEvents
                .Where(e => plantIds.Contains(e.PlantId))
                .OrderByDescending(e => e.TimestampUtc)
                .Select(e => e.TimestampUtc)
                .FirstOrDefaultAsync();

            if (last != default && (now - last).TotalMinutes < 5)
                return;

            foreach (var plantId in plantIds)
            {
                _db.WateringEvents.Add(new WateringEvent
                {
                    PlantId = plantId,
                    TimestampUtc = now
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
