namespace BMATM.Services;
public interface IATMService
{
    Task InitializeAsync();
    Task<List<ATMInfo>> GetATMsByUsernameAsync(string username);
    Task<List<ATMInfo>> GetATMsByBranchAsync(string branchCode);
    Task<ATMInfo> GetATMByNumberAsync(string atmNumber);
    Task<bool> CreateATMAsync(ATMInfo atmInfo);
    Task<bool> UpdateATMAsync(ATMInfo atmInfo);
    Task<bool> DeactivateATMAsync(string atmNumber);
    Task<List<ATMInfo>> GetAllATMsAsync();
}