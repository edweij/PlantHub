using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;

namespace PlantHub.Web.Infrastructure;

public interface IWateringGroupService
{
    Task DeleteAsync(WateringGroup wateringGroup, CancellationToken ct = default);
    Task<WateringGroup> AddAsync(WateringGroup group, CancellationToken ct = default);
    Task<IReadOnlyList<WateringGroup>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WateringGroup>> GetAllWithPlantsAsync(CancellationToken ct = default);
    Task<WateringGroup> UpdateAsync(WateringGroup group, CancellationToken ct = default);
}

public class WateringGroupService : IWateringGroupService
{
    private readonly PlantHubDbContext _db;

    public WateringGroupService(PlantHubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<WateringGroup>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.WateringGroups
            .OrderBy(g => g.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WateringGroup>> GetAllWithPlantsAsync(CancellationToken ct = default)
    {
        return await _db.WateringGroups
            .Include(g => g.Plants)
                .ThenInclude(p => p.WateringEvents)
            .AsSplitQuery()
            .OrderBy(g => g.IntervalDaysSummer)
            .ToListAsync(ct);
    }

    public async Task<WateringGroup> AddAsync(WateringGroup group, CancellationToken ct = default)
    {
        if (group is null) throw new ArgumentNullException(nameof(group));

        _db.WateringGroups.Add(group);
        await _db.SaveChangesAsync(ct);
        return group;
    }

    public Task DeleteAsync(WateringGroup wateringGroup, CancellationToken ct = default)
    {
        _db.WateringGroups.Remove(wateringGroup);
        return _db.SaveChangesAsync(ct);
    }

    public async Task<WateringGroup> UpdateAsync(WateringGroup group, CancellationToken ct = default)
    {
        _db.WateringGroups.Update(group);
        await _db.SaveChangesAsync(ct);
        return group;
    }
}
