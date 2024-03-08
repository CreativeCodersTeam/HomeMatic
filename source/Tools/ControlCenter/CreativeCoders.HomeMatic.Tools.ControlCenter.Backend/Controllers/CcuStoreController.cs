using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Controllers;

[ApiController]
[Route("ccu")]
[Authorize(Policy = "RestrictedApi")]
public class CcuStoreController : ControllerBase
{
    private readonly ICcuRepository _ccuRepository;

    public CcuStoreController(ICcuRepository ccuRepository)
    {
        _ccuRepository = ccuRepository;
    }

    [HttpGet(Name = "GetCcus")]
    public async Task<IEnumerable<CcuModel>> GetAsync()
    {
        return await _ccuRepository.GetAllAsync();
    }

    [HttpPost(Name = "AddCcu")]
    public async Task<IActionResult> PostAsync(CcuModel ccu)
    {
        ccu.Id = Guid.NewGuid().ToString();

        await _ccuRepository.AddAsync(ccu);
        return Ok(ccu);
    }

    [HttpPut(Name = "UpdateCcu")]
    public async Task<IActionResult> PutAsync(CcuModel ccu)
    {
        await _ccuRepository.UpdateAsync(ccu);
        return Ok();
    }

    [HttpDelete("{id}", Name = "DeleteCcu")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _ccuRepository.RemoveAsync(id);
        return Ok();
    }
}
