namespace BMATM.Services;

public class ATMService : IATMService
{
    private readonly IATMRepository _atmRepository;

    public ATMService(IATMRepository atmRepository)
    {
        _atmRepository = atmRepository;
    }

    public async Task InitializeAsync()
    {
        await _atmRepository.InitializeAsync().ConfigureAwait(false);
    }
    public async Task<List<ATMInfo>> GetATMsByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<ATMInfo>();

        return await _atmRepository.GetByUsernameAsync(username).ConfigureAwait(false);
    }
    public async Task<List<ATMInfo>> GetATMsByBranchAsync(string branchCode)
    {
        if (string.IsNullOrWhiteSpace(branchCode))
            return new List<ATMInfo>();

        return await _atmRepository.GetByBranchCodeAsync(branchCode).ConfigureAwait(false);
    }

    public async Task<ATMInfo> GetATMByNumberAsync(string atmNumber)
    {
        if (string.IsNullOrWhiteSpace(atmNumber))
            return null;

        return await _atmRepository.GetByATMNumberAsync(atmNumber).ConfigureAwait(false);
    }

    public async Task<bool> CreateATMAsync(ATMInfo atmInfo)
    {
        if (atmInfo == null || string.IsNullOrWhiteSpace(atmInfo.ATMNumber) || string.IsNullOrWhiteSpace(atmInfo.Username))
            return false;

        return await _atmRepository.CreateATMAsync(atmInfo).ConfigureAwait(false);
    }

    public async Task<bool> UpdateATMAsync(ATMInfo atmInfo)
    {
        if (atmInfo == null || string.IsNullOrWhiteSpace(atmInfo.ATMNumber))
            return false;

        return await _atmRepository.UpdateATMAsync(atmInfo).ConfigureAwait(false);
    }

    public async Task<bool> DeactivateATMAsync(string atmNumber)
    {
        if (string.IsNullOrWhiteSpace(atmNumber))
            return false;

        return await _atmRepository.DeleteATMAsync(atmNumber).ConfigureAwait(false);
    }

    public async Task<List<ATMInfo>> GetAllATMsAsync()
    {
        return await _atmRepository.GetAllATMsAsync().ConfigureAwait(false);
    }    
}
