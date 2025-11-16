using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;
using PlantHub.Web.Infrastructure;
using PlantHub.Web.Services;

namespace PlantHub.Infrastructure;

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
}
