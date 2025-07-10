namespace BMATM.Services;

// Updated ATM Data Service using Repository
public class SQLiteATMDataService : IATMDataService
{
    private readonly IATMRepository _atmRepository;
    private readonly DatabaseContext _context;

    public SQLiteATMDataService(IATMRepository atmRepository, DatabaseContext context)
    {
        _atmRepository = atmRepository;
        _context = context;
    }

    public async Task<List<ATMInfo>> GetAllATMsAsync()
    {
        var atms = await _atmRepository.GetAllAsync();
        return atms.ToList();
    }

    public async Task<ATMInfo> GetATMByNumberAsync(string atmNumber)
    {
        return await _atmRepository.GetByATMNumberAsync(atmNumber);
    }

    public async Task<bool> AddATMAsync(ATMInfo atm)
    {
        try
        {
            if (await _atmRepository.ExistsByATMNumberAsync(atm.ATMNumber))
            {
                return false; // ATM already exists
            }

            var result = await _atmRepository.AddAsync(atm);
            return result != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UpdateATMAsync(ATMInfo atm)
    {
        try
        {
            if (!await _atmRepository.ExistsByATMNumberAsync(atm.ATMNumber))
            {
                return false; // ATM doesn't exist
            }

            var result = await _atmRepository.UpdateAsync(atm);
            return result != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteATMAsync(string atmNumber)
    {
        try
        {
            var atm = await _atmRepository.GetByATMNumberAsync(atmNumber);
            if (atm == null)
            {
                return false;
            }
                        
            return await _atmRepository.DeleteByATMNumberAsync(atmNumber);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ATMExistsAsync(string atmNumber)
    {
        return await _atmRepository.ExistsByATMNumberAsync(atmNumber);
    }

    // Additional methods leveraging repository capabilities
    public async Task<List<ATMInfo>> GetATMsByBranchAsync(string branchCode)
    {
        var atms = await _atmRepository.GetByBranchCodeAsync(branchCode);
        return atms.ToList();
    }

    public async Task<List<ATMInfo>> GetActiveATMsAsync()
    {
        var atms = await _atmRepository.GetActiveATMsAsync();
        return atms.ToList();
    }

    public async Task<bool> UpdateATMBalanceAsync(string atmNumber, int cassette1Balance, int cassette2Balance, int cassette3Balance, int cassette4Balance)
    {
        return await _atmRepository.UpdateBalanceAsync(atmNumber, cassette1Balance, cassette2Balance, cassette3Balance, cassette4Balance);
    }

    public async Task<bool> UpdateATMMaintenanceDateAsync(string atmNumber, DateTime maintenanceDate)
    {
        return await _atmRepository.UpdateMaintenanceDateAsync(atmNumber, maintenanceDate);
    }
}