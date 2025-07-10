namespace BMATM.Services;
public interface IATMDataService
{
    Task<List<ATMInfo>> GetAllATMsAsync();
    Task<ATMInfo> GetATMByNumberAsync(string atmNumber);
    Task<bool> AddATMAsync(ATMInfo atm);
    Task<bool> UpdateATMAsync(ATMInfo atm);
    Task<bool> DeleteATMAsync(string atmNumber);
    Task<bool> ATMExistsAsync(string atmNumber);
}