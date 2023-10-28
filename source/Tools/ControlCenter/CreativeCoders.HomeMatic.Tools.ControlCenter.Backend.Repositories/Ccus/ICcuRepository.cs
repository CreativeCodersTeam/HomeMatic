namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;

public interface ICcuRepository
{
    Task AddAsync(CcuModel ccu);
    
    Task<IEnumerable<CcuModel>> GetAllAsync();
    
    Task RemoveAsync(string id);
    
    Task UpdateAsync(CcuModel ccu);
}