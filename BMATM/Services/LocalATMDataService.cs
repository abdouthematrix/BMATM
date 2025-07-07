using System.Configuration;

namespace BMATM.Services;

public class LocalATMDataService : IATMDataService
{
    private readonly string _dataFilePath;
    private List<ATMInfo> _atmCache;

    public LocalATMDataService()
    {
        var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
        //var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var bmAtmPath = Path.GetDirectoryName(path); //Path.Combine(appDataPath, "BMATM");

        if (!Directory.Exists(bmAtmPath))
        {
            Directory.CreateDirectory(bmAtmPath);
        }

        _dataFilePath = Path.Combine(bmAtmPath, "atm_data.json");
        _atmCache = new List<ATMInfo>();
    }

    public async Task<List<ATMInfo>> GetAllATMsAsync()
    {
        if (_atmCache.Any())
        {
            return _atmCache;
        }

        if (!File.Exists(_dataFilePath))
        {
            _atmCache = GetDefaultATMs();
            await SaveATMsAsync();
            return _atmCache;
        }

        try
        {
            var jsonString = await File.ReadAllTextAsync(_dataFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            _atmCache = JsonSerializer.Deserialize<List<ATMInfo>>(jsonString, options) ?? new List<ATMInfo>();

            if (!_atmCache.Any())
            {
                _atmCache = GetDefaultATMs();
                await SaveATMsAsync();
            }
        }
        catch (Exception)
        {
            _atmCache = GetDefaultATMs();
            await SaveATMsAsync();
        }

        return _atmCache;
    }

    public async Task<ATMInfo> GetATMByNumberAsync(string atmNumber)
    {
        var atms = await GetAllATMsAsync();
        return atms.FirstOrDefault(a => a.ATMNumber.Equals(atmNumber, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> AddATMAsync(ATMInfo atm)
    {
        try
        {
            var atms = await GetAllATMsAsync();

            if (atms.Any(a => a.ATMNumber.Equals(atm.ATMNumber, StringComparison.OrdinalIgnoreCase)))
            {
                return false; // ATM already exists
            }

            atms.Add(atm);
            _atmCache = atms;
            await SaveATMsAsync();
            return true;
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
            var atms = await GetAllATMsAsync();
            var existingATM = atms.FirstOrDefault(a => a.ATMNumber.Equals(atm.ATMNumber, StringComparison.OrdinalIgnoreCase));

            if (existingATM == null)
            {
                return false;
            }

            var index = atms.IndexOf(existingATM);
            atms[index] = atm;
            _atmCache = atms;
            await SaveATMsAsync();
            return true;
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
            var atms = await GetAllATMsAsync();
            var atmToRemove = atms.FirstOrDefault(a => a.ATMNumber.Equals(atmNumber, StringComparison.OrdinalIgnoreCase));

            if (atmToRemove == null)
            {
                return false;
            }

            atms.Remove(atmToRemove);
            _atmCache = atms;
            await SaveATMsAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ATMExistsAsync(string atmNumber)
    {
        var atms = await GetAllATMsAsync();
        return atms.Any(a => a.ATMNumber.Equals(atmNumber, StringComparison.OrdinalIgnoreCase));
    }

    private async Task SaveATMsAsync()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var jsonString = JsonSerializer.Serialize(_atmCache, options);
            await File.WriteAllTextAsync(_dataFilePath, jsonString);
        }
        catch (Exception)
        {
            // Handle save error silently or log if needed
        }
    }

    private List<ATMInfo> GetDefaultATMs()
    {
        return new List<ATMInfo>
       {
           new ATMInfo
           {
               ATMNumber = "ATM001",
               ATMType = ATMType.NCR,
               GLAccount = "GL001",
               BranchCode = "BR001",
               BranchName = "Main Branch",
               Location = "Downtown Plaza",
               Cassette1Denomination = 20,
               Cassette2Denomination = 50,
               Cassette3Denomination = 100,
               Cassette4Denomination = 200,
               Cassette1Balance = 50000,
               Cassette2Balance = 100000,
               Cassette3Balance = 200000,
               Cassette4Balance = 400000,
               IPAddress = "192.168.1.100",
               IsActive = true,
               InstallationDate = DateTime.Now.AddYears(-2),
               LastMaintenanceDate = DateTime.Now.AddMonths(-3),
               Notes = "Main branch ATM - high traffic location"
           },
           new ATMInfo
           {
               ATMNumber = "ATM002",
               ATMType = ATMType.DN,
               GLAccount = "GL002",
               BranchCode = "BR002",
               BranchName = "Mall Branch",
               Location = "Shopping Mall - Level 1",
               Cassette1Denomination = 20,
               Cassette2Denomination = 50,
               Cassette3Denomination = 100,
               Cassette4Denomination = 200,
               Cassette1Balance = 30000,
               Cassette2Balance = 75000,
               Cassette3Balance = 150000,
               Cassette4Balance = 300000,
               IPAddress = "192.168.1.101",
               IsActive = true,
               InstallationDate = DateTime.Now.AddYears(-1),
               LastMaintenanceDate = DateTime.Now.AddMonths(-2),
               Notes = "Mall location - weekend peak usage"
           }
       };
    }
}