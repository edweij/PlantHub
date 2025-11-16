using PlantHub.Web.Domain;

namespace PlantHub.Web.Services;

public interface IWateringGroupService
{
    Task<IReadOnlyList<WateringGroup>> GetAllAsync(CancellationToken ct = default);
}
