﻿using CreativeCoders.Core.Collections;
using CreativeCoders.Core.Threading;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;

public interface ICcuRepository
{
    Task AddAsync(CcuModel ccu);
    
    Task<IEnumerable<CcuModel>> GetAllAsync();
    
    Task RemoveAsync(string id);
    
    Task UpdateAsync(CcuModel ccu);
}

public class InMemoryCcuRepository : ICcuRepository
{
    private readonly ConcurrentList<CcuModel> _ccuList = new();

    public Task AddAsync(CcuModel ccu)
    {
        _ccuList.Add(ccu);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<CcuModel>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<CcuModel>>(_ccuList);
    }

    public Task RemoveAsync(string id)
    {
        _ccuList.Remove(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CcuModel ccu)
    {
        var foundCcu = _ccuList.SingleOrDefault(x => x.Id == ccu.Id);

        if (foundCcu == null)
        {
            return AddAsync(ccu);
        }
        
        foundCcu.Name = ccu.Name;
        foundCcu.Url = ccu.Url;
        
        return Task.CompletedTask;
    }
}

public class CcuModel
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public Uri? Url { get; set; }
}
