namespace BMATM.Data.Repositories;
public interface IATMRepository : IBaseRepository<ATMInfo>
{
    Task<ATMInfo?> GetByATMNumberAsync(string atmNumber);
    Task<bool> ExistsByATMNumberAsync(string atmNumber);
    Task<IEnumerable<ATMInfo>> GetByBranchCodeAsync(string branchCode);
    Task<IEnumerable<ATMInfo>> GetActiveATMsAsync();
    Task<bool> UpdateBalanceAsync(string atmNumber, int cassette1Balance, int cassette2Balance, int cassette3Balance, int cassette4Balance);
    Task<bool> UpdateMaintenanceDateAsync(string atmNumber, DateTime maintenanceDate);
    Task<bool> DeleteByATMNumberAsync(string atmNumber); // New method
}
public class ATMRepository : BaseRepository<ATMInfo>, IATMRepository
{    
    public ATMRepository(DatabaseContext context) : base(context, "ATMs")
    {
    }

    public async Task<ATMInfo?> GetByATMNumberAsync(string atmNumber)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE ATMNumber = @ATMNumber COLLATE NOCASE";
        command.Parameters.AddWithValue("@ATMNumber", atmNumber);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return await MapReaderToEntityAsync(reader);
        }

        return null;
    }

    public async Task<bool> ExistsByATMNumberAsync(string atmNumber)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {_tableName} WHERE ATMNumber = @ATMNumber COLLATE NOCASE";
        command.Parameters.AddWithValue("@ATMNumber", atmNumber);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<bool> DeleteByATMNumberAsync(string atmNumber)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {_tableName} WHERE ATMNumber = @ATMNumber COLLATE NOCASE";
        command.Parameters.AddWithValue("@ATMNumber", atmNumber);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<ATMInfo>> GetByBranchCodeAsync(string branchCode)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE BranchCode = @BranchCode ORDER BY ATMNumber";
        command.Parameters.AddWithValue("@BranchCode", branchCode);

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<ATMInfo>();

        while (await reader.ReadAsync())
        {
            var entity = await MapReaderToEntityAsync(reader);
            if (entity != null)
                results.Add(entity);
        }

        return results;
    }

    public async Task<IEnumerable<ATMInfo>> GetActiveATMsAsync()
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE IsActive = 1 ORDER BY ATMNumber";

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<ATMInfo>();

        while (await reader.ReadAsync())
        {
            var entity = await MapReaderToEntityAsync(reader);
            if (entity != null)
                results.Add(entity);
        }

        return results;
    }

    public async Task<bool> UpdateBalanceAsync(string atmNumber, int cassette1Balance, int cassette2Balance, int cassette3Balance, int cassette4Balance)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            UPDATE {_tableName} 
            SET Cassette1Balance = @Cassette1Balance,
                Cassette2Balance = @Cassette2Balance,
                Cassette3Balance = @Cassette3Balance,
                Cassette4Balance = @Cassette4Balance
            WHERE ATMNumber = @ATMNumber COLLATE NOCASE";

        command.Parameters.AddWithValue("@ATMNumber", atmNumber);
        command.Parameters.AddWithValue("@Cassette1Balance", cassette1Balance);
        command.Parameters.AddWithValue("@Cassette2Balance", cassette2Balance);
        command.Parameters.AddWithValue("@Cassette3Balance", cassette3Balance);
        command.Parameters.AddWithValue("@Cassette4Balance", cassette4Balance);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateMaintenanceDateAsync(string atmNumber, DateTime maintenanceDate)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            UPDATE {_tableName} 
            SET LastMaintenanceDate = @LastMaintenanceDate
            WHERE ATMNumber = @ATMNumber COLLATE NOCASE";

        command.Parameters.AddWithValue("@ATMNumber", atmNumber);
        command.Parameters.AddWithValue("@LastMaintenanceDate", maintenanceDate.ToString("yyyy-MM-dd HH:mm:ss"));

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    protected override async Task<ATMInfo?> MapReaderToEntityAsync(SqliteDataReader reader)
    {
        return new ATMInfo
        {
            ATMNumber = GetString(reader, "ATMNumber"),
            ATMType = (ATMType)GetInt32(reader, "ATMType"),
            GLAccount = GetString(reader, "GLAccount"),
            BranchCode = GetString(reader, "BranchCode"),
            BranchName = GetString(reader, "BranchName"),
            Location = GetString(reader, "Location"),
            Cassette1Denomination = GetInt32(reader, "Cassette1Denomination"),
            Cassette2Denomination = GetInt32(reader, "Cassette2Denomination"),
            Cassette3Denomination = GetInt32(reader, "Cassette3Denomination"),
            Cassette4Denomination = GetInt32(reader, "Cassette4Denomination"),
            Cassette1Balance = GetInt32(reader, "Cassette1Balance"),
            Cassette2Balance = GetInt32(reader, "Cassette2Balance"),
            Cassette3Balance = GetInt32(reader, "Cassette3Balance"),
            Cassette4Balance = GetInt32(reader, "Cassette4Balance"),
            IPAddress = GetString(reader, "IPAddress"),
            IsActive = GetBoolean(reader, "IsActive"),
            InstallationDate = GetDateTimeOrDefault(reader, "InstallationDate"),
            LastMaintenanceDate = GetDateTimeOrDefault(reader, "LastMaintenanceDate"),
            Notes = GetString(reader, "Notes")
        };
    }

    protected override (string sql, Dictionary<string, object?> parameters) GetInsertSqlAndParameters(ATMInfo entity)
    {
        var sql = @$"
            INSERT INTO {_tableName} (ATMNumber, ATMType, GLAccount, BranchCode, BranchName, Location,
                            Cassette1Denomination, Cassette2Denomination, Cassette3Denomination, Cassette4Denomination,
                            Cassette1Balance, Cassette2Balance, Cassette3Balance, Cassette4Balance,
                            IPAddress, IsActive, InstallationDate, LastMaintenanceDate, Notes)
            VALUES (@ATMNumber, @ATMType, @GLAccount, @BranchCode, @BranchName, @Location,
                   @Cassette1Denomination, @Cassette2Denomination, @Cassette3Denomination, @Cassette4Denomination,
                   @Cassette1Balance, @Cassette2Balance, @Cassette3Balance, @Cassette4Balance,
                   @IPAddress, @IsActive, @InstallationDate, @LastMaintenanceDate, @Notes);
            SELECT last_insert_rowid();";

        var parameters = new Dictionary<string, object?>
        {
            ["@ATMNumber"] = entity.ATMNumber,
            ["@ATMType"] = (int)entity.ATMType,
            ["@GLAccount"] = entity.GLAccount,
            ["@BranchCode"] = entity.BranchCode,
            ["@BranchName"] = entity.BranchName,
            ["@Location"] = entity.Location,
            ["@Cassette1Denomination"] = entity.Cassette1Denomination,
            ["@Cassette2Denomination"] = entity.Cassette2Denomination,
            ["@Cassette3Denomination"] = entity.Cassette3Denomination,
            ["@Cassette4Denomination"] = entity.Cassette4Denomination,
            ["@Cassette1Balance"] = entity.Cassette1Balance,
            ["@Cassette2Balance"] = entity.Cassette2Balance,
            ["@Cassette3Balance"] = entity.Cassette3Balance,
            ["@Cassette4Balance"] = entity.Cassette4Balance,
            ["@IPAddress"] = entity.IPAddress,
            ["@IsActive"] = entity.IsActive,
            ["@InstallationDate"] = entity.InstallationDate.ToString("yyyy-MM-dd HH:mm:ss"),
            ["@LastMaintenanceDate"] = entity.LastMaintenanceDate.ToString("yyyy-MM-dd HH:mm:ss"),
            ["@Notes"] = entity.Notes
        };

        return (sql, parameters);
    }

    protected override int GetEntityId(ATMInfo entity)
    {
        // Since ATMInfo doesn't have an Id property, we'll need to get it from the database
        // This is a limitation of the current ATMInfo model structure
        throw new NotImplementedException("ATMInfo model needs an Id property for this operation");
    }

    protected override (string sql, Dictionary<string, object?> parameters) GetUpdateSqlAndParameters(ATMInfo entity)
    {
        var sql = @$"
            UPDATE {_tableName} 
            SET ATMType = @ATMType,
                GLAccount = @GLAccount,
                BranchCode = @BranchCode,
                BranchName = @BranchName,
                Location = @Location,
                Cassette1Denomination = @Cassette1Denomination,
                Cassette2Denomination = @Cassette2Denomination,
                Cassette3Denomination = @Cassette3Denomination,
                Cassette4Denomination = @Cassette4Denomination,
                Cassette1Balance = @Cassette1Balance,
                Cassette2Balance = @Cassette2Balance,
                Cassette3Balance = @Cassette3Balance,
                Cassette4Balance = @Cassette4Balance,
                IPAddress = @IPAddress,
                IsActive = @IsActive,
                InstallationDate = @InstallationDate,
                LastMaintenanceDate = @LastMaintenanceDate,
                Notes = @Notes
            WHERE ATMNumber = @ATMNumber COLLATE NOCASE";

        var parameters = new Dictionary<string, object?>
        {
            ["@ATMNumber"] = entity.ATMNumber,
            ["@ATMType"] = (int)entity.ATMType,
            ["@GLAccount"] = entity.GLAccount,
            ["@BranchCode"] = entity.BranchCode,
            ["@BranchName"] = entity.BranchName,
            ["@Location"] = entity.Location,
            ["@Cassette1Denomination"] = entity.Cassette1Denomination,
            ["@Cassette2Denomination"] = entity.Cassette2Denomination,
            ["@Cassette3Denomination"] = entity.Cassette3Denomination,
            ["@Cassette4Denomination"] = entity.Cassette4Denomination,
            ["@Cassette1Balance"] = entity.Cassette1Balance,
            ["@Cassette2Balance"] = entity.Cassette2Balance,
            ["@Cassette3Balance"] = entity.Cassette3Balance,
            ["@Cassette4Balance"] = entity.Cassette4Balance,
            ["@IPAddress"] = entity.IPAddress,
            ["@IsActive"] = entity.IsActive,
            ["@InstallationDate"] = entity.InstallationDate.ToString("yyyy-MM-dd HH:mm:ss"),
            ["@LastMaintenanceDate"] = entity.LastMaintenanceDate.ToString("yyyy-MM-dd HH:mm:ss"),
            ["@Notes"] = entity.Notes
        };

        return (sql, parameters);
    }
}

        