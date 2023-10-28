using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;

public class ObjectCcuRepository : ICcuRepository
{
    private readonly IObjectRepository<CcuModel, string> _objectRepository;

    public ObjectCcuRepository(IObjectRepository<CcuModel, string> objectRepository)
    {
        _objectRepository = Ensure.NotNull(objectRepository);
    }
    
    public Task AddAsync(CcuModel ccu)
    {
        return _objectRepository.AddAsync(ccu);
    }

    public Task<IEnumerable<CcuModel>> GetAllAsync()
    {
        return _objectRepository.GetAllAsync();
    }

    public Task RemoveAsync(string id)
    {
        return _objectRepository.DeleteAsync(id);
    }

    public Task UpdateAsync(CcuModel ccu)
    {
        return _objectRepository.UpdateAsync(ccu);
    }
}
