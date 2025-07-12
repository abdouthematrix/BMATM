namespace BMATM.Data;

public class ATMRepository : BaseRepository, IATMRepository
{
    public static string RepositoryName = "ATMs";
    public ATMRepository(DatabaseContext context) : base(context, RepositoryName) { }

    public override async Task InitializeAsync()
    {
        if (!await TableExistsAsync().ConfigureAwait(false))
        {
             string createTableQuery = @$"
                    CREATE TABLE {TableName} (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ATMNumber TEXT NOT NULL UNIQUE,
                        ATMType INTEGER NOT NULL,
                        GLAccount TEXT,
                        BranchCode TEXT NOT NULL,
                        BranchName TEXT,                       
                        Cassette1Denomination INTEGER DEFAULT 0,
                        Cassette2Denomination INTEGER DEFAULT 0,
                        Cassette3Denomination INTEGER DEFAULT 0,
                        Cassette4Denomination INTEGER DEFAULT 0,  
                        IsActive BOOLEAN NOT NULL DEFAULT 1,                       
                        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        Username TEXT NOT NULL,
                        FOREIGN KEY (Username) REFERENCES {UserRepository.RepositoryName}(Username)
                    )";

            await _context.ExecuteNonQueryAsync(createTableQuery).ConfigureAwait(false);
         //   await SeedSampleATMsAsync().ConfigureAwait(false);
        }
    }
    public async Task<List<ATMInfo>> GetByUsernameAsync(string username)
    {
        _queryHelper.Reset();
        var query = _queryHelper.Select().Where("Username", username).Where("IsActive", true).OrderBy("ATMNumber").Build();
        var parameters = _queryHelper.GetParameters();

        return await _context.ExecuteQueryAsync(query, parameters, MapATM).ConfigureAwait(false);
    }

    public async Task<List<ATMInfo>> GetByBranchCodeAsync(string branchCode)
    {
        _queryHelper.Reset();
        var query = _queryHelper.Select().Where("BranchCode", branchCode).Where("IsActive", true).OrderBy("ATMNumber").Build();
        var parameters = _queryHelper.GetParameters();

        return await _context.ExecuteQueryAsync(query, parameters, MapATM).ConfigureAwait(false);
    }

    public async Task<ATMInfo> GetByATMNumberAsync(string atmNumber)
    {
        _queryHelper.Reset();
        var query = _queryHelper.Select().Where("ATMNumber", atmNumber).Build();
        var parameters = _queryHelper.GetParameters();

        var atms = await _context.ExecuteQueryAsync(query, parameters, MapATM).ConfigureAwait(false);
        return atms.FirstOrDefault();
    }

    public async Task<bool> CreateATMAsync(ATMInfo atmInfo)
    {
        _queryHelper.Reset();
        var values = new Dictionary<string, object>
            {
                { "ATMNumber", atmInfo.ATMNumber },
                { "ATMType", (int)atmInfo.ATMType },
                { "GLAccount", atmInfo.GLAccount },
                { "BranchCode", atmInfo.BranchCode },
                { "BranchName", atmInfo.BranchName },
                { "Cassette1Denomination", atmInfo.Cassette1Denomination },
                { "Cassette2Denomination", atmInfo.Cassette2Denomination },
                { "Cassette3Denomination", atmInfo.Cassette3Denomination },
                { "Cassette4Denomination", atmInfo.Cassette4Denomination },               
                { "IsActive", atmInfo.IsActive },               
                { "Username", atmInfo.Username }
            };

        var query = _queryHelper.Insert().Values(values).Build();
        var parameters = _queryHelper.GetParameters();

        try
        {
            var result = await _context.ExecuteNonQueryAsync(query, parameters).ConfigureAwait(false);
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateATMAsync(ATMInfo atmInfo)
    {
        _queryHelper.Reset();
        var values = new Dictionary<string, object>
            {
                { "ATMType", (int)atmInfo.ATMType },
                { "GLAccount", atmInfo.GLAccount },
                { "BranchCode", atmInfo.BranchCode },
                { "BranchName", atmInfo.BranchName },
                { "Cassette1Denomination", atmInfo.Cassette1Denomination },
                { "Cassette2Denomination", atmInfo.Cassette2Denomination },
                { "Cassette3Denomination", atmInfo.Cassette3Denomination },
                { "Cassette4Denomination", atmInfo.Cassette4Denomination },                
                { "IsActive", atmInfo.IsActive },               
                { "Username", atmInfo.Username }
            };

        var query = _queryHelper.Update().Set(values).Where("ATMNumber", atmInfo.ATMNumber).Build();
        var parameters = _queryHelper.GetParameters();

        var result = await _context.ExecuteNonQueryAsync(query, parameters).ConfigureAwait(false);
        return result > 0;
    }
    public async Task<bool> DeleteATMAsync(string atmNumber)
    {
        _queryHelper.Reset();
        var query = _queryHelper.Delete().Where("ATMNumber", atmNumber).Build();
        var parameters = _queryHelper.GetParameters();

        var result = await _context.ExecuteNonQueryAsync(query, parameters).ConfigureAwait(false);
        return result > 0;
    }
    public async Task<List<ATMInfo>> GetAllATMsAsync()
    {
        _queryHelper.Reset();
        var query = _queryHelper.Select().Where("IsActive", true).OrderBy("BranchCode").OrderBy("ATMNumber").Build();
        var parameters = _queryHelper.GetParameters();

        return await _context.ExecuteQueryAsync(query, parameters, MapATM).ConfigureAwait(false);
    }    
    private ATMInfo MapATM(SqliteDataReader reader)
    {
        return new ATMInfo
        {
            Username = reader["Username"].ToString(),
            ATMNumber = reader["ATMNumber"].ToString(),
            ATMType = (ATMType)Convert.ToInt32(reader["ATMType"]),
            GLAccount = reader["GLAccount"].ToString(),
            BranchCode = reader["BranchCode"].ToString(),
            BranchName = reader["BranchName"].ToString(),
            Cassette1Denomination = Convert.ToInt32(reader["Cassette1Denomination"]),
            Cassette2Denomination = Convert.ToInt32(reader["Cassette2Denomination"]),
            Cassette3Denomination = Convert.ToInt32(reader["Cassette3Denomination"]),
            Cassette4Denomination = Convert.ToInt32(reader["Cassette4Denomination"]),
            IsActive = Convert.ToBoolean(reader["IsActive"]),
        };
    }
}