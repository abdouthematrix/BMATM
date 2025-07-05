using System.Configuration;

namespace BMATM.Services;

public class LocalUserDataService : IUserDataService
{
    private readonly string _dataDirectory;
    private readonly string _profilesFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public LocalUserDataService()
    {
        var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
        _dataDirectory = Path.GetDirectoryName(path); //Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BMATM");
        _profilesFilePath = Path.Combine(_dataDirectory, "supervisor_profiles.json");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        EnsureDataDirectoryExists();
    }

    private void EnsureDataDirectoryExists()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    public async Task<SupervisorProfile?> GetSupervisorProfileAsync(string username)
    {
        try
        {
            var profiles = await LoadProfilesAsync();
            return profiles.FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SaveSupervisorProfileAsync(SupervisorProfile profile)
    {
        try
        {
            var profiles = await LoadProfilesAsync();

            // Check if profile already exists
            var existingProfileIndex = profiles.FindIndex(p => p.Username.Equals(profile.Username, StringComparison.OrdinalIgnoreCase));

            if (existingProfileIndex >= 0)
            {
                // Update existing profile
                profiles[existingProfileIndex] = profile;
            }
            else
            {
                // Add new profile
                profiles.Add(profile);
            }

            await SaveProfilesAsync(profiles);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateSupervisorProfileAsync(SupervisorProfile profile)
    {
        return await SaveSupervisorProfileAsync(profile);
    }

    public async Task<bool> DeleteSupervisorProfileAsync(string username)
    {
        try
        {
            var profiles = await LoadProfilesAsync();
            var profileToRemove = profiles.FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (profileToRemove != null)
            {
                profiles.Remove(profileToRemove);
                await SaveProfilesAsync(profiles);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<SupervisorProfile>> GetAllSupervisorProfilesAsync()
    {
        try
        {
            return await LoadProfilesAsync();
        }
        catch
        {
            return new List<SupervisorProfile>();
        }
    }

    public async Task<bool> ProfileExistsAsync(string username)
    {
        try
        {
            var profiles = await LoadProfilesAsync();
            return profiles.Any(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetAtmCountForSupervisorAsync(string username)
    {
        try
        {
            var profile = await GetSupervisorProfileAsync(username);
            return profile?.AtmCollectionCount ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<bool> UpdateAtmCountAsync(string username, int newCount)
    {
        try
        {
            var profile = await GetSupervisorProfileAsync(username);
            if (profile != null)
            {
                profile.AtmCollectionCount = newCount;
                return await SaveSupervisorProfileAsync(profile);
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<DateTime?> GetLastLoginDateAsync(string username)
    {
        try
        {
            var profile = await GetSupervisorProfileAsync(username);
            return profile?.LastLoginDate;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateLastLoginDateAsync(string username, DateTime loginDate)
    {
        try
        {
            var profile = await GetSupervisorProfileAsync(username);
            if (profile != null)
            {
                profile.LastLoginDate = loginDate;
                return await SaveSupervisorProfileAsync(profile);
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
          
    public async Task<bool> ClearAllDataAsync()
    {
        try
        {
            if (File.Exists(_profilesFilePath))
            {
                File.Delete(_profilesFilePath);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<SupervisorProfile>> LoadProfilesAsync()
    {
        try
        {
            if (!File.Exists(_profilesFilePath))
            {
                return new List<SupervisorProfile>();
            }

            var jsonContent = await File.ReadAllTextAsync(_profilesFilePath);
            var profiles = JsonSerializer.Deserialize<List<SupervisorProfile>>(jsonContent, _jsonOptions);
            return profiles ?? new List<SupervisorProfile>();
        }
        catch
        {
            return new List<SupervisorProfile>();
        }
    }

    private async Task SaveProfilesAsync(List<SupervisorProfile> profiles)
    {
        var jsonContent = JsonSerializer.Serialize(profiles, _jsonOptions);
        await File.WriteAllTextAsync(_profilesFilePath, jsonContent);
    }
}