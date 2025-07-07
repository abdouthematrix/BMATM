namespace BMATM.Services;

public interface IUserDataService
{
    Task<SupervisorProfile?> GetSupervisorProfileAsync(string username);
    Task<bool> SaveSupervisorProfileAsync(SupervisorProfile profile);
    Task<bool> UpdateSupervisorProfileAsync(SupervisorProfile profile);
    Task<bool> DeleteSupervisorProfileAsync(string username);
    Task<List<SupervisorProfile>> GetAllSupervisorProfilesAsync();
    Task<bool> ProfileExistsAsync(string username); 
    Task<DateTime?> GetLastLoginDateAsync(string username);
    Task<bool> UpdateLastLoginDateAsync(string username, DateTime loginDate);    
    Task<bool> ClearAllDataAsync();
}