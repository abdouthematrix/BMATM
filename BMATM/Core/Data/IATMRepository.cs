namespace BMATM.Data;

// ==================== ATM REPOSITORY ====================
public interface IATMRepository
{
    Task InitializeAsync();
    Task<List<ATMInfo>> GetByUsernameAsync(string username);
    Task<List<ATMInfo>> GetByBranchCodeAsync(string branchCode);
    Task<ATMInfo> GetByATMNumberAsync(string atmNumber);
    Task<bool> CreateATMAsync(ATMInfo atmInfo);
    Task<bool> UpdateATMAsync(ATMInfo atmInfo);
    Task<bool> DeleteATMAsync(string atmNumber);
    Task<List<ATMInfo>> GetAllATMsAsync();
}