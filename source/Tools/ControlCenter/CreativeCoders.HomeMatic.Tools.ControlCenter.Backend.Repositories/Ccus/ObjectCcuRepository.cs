using CreativeCoders.Core;
using CreativeCoders.Data.NoSql;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;

public class ObjectCcuRepository : ICcuRepository
{
    private readonly IDocumentRepository<CcuModel, string> _documentRepository;

    public ObjectCcuRepository(IDocumentRepository<CcuModel, string> documentRepository)
    {
        _documentRepository = Ensure.NotNull(documentRepository);
    }
    
    public Task AddAsync(CcuModel ccu)
    {
        return _documentRepository.AddAsync(ccu);
    }

    public Task<IEnumerable<CcuModel>> GetAllAsync()
    {
        return _documentRepository.GetAllAsync();
    }

    public Task RemoveAsync(string id)
    {
        return _documentRepository.DeleteAsync(id);
    }

    public Task UpdateAsync(CcuModel ccu)
    {
        return _documentRepository.UpdateAsync(ccu);
    }
}
