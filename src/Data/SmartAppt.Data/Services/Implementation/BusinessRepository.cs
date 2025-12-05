using Data.SmartAppt.SQL.Configs;
using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation;

public class BusinessRepository : IBusinessRepository
{
    private readonly string? _connectionString;

    public BusinessRepository(IOptions<DataBaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    private static object DbNullIfNull(object? value) => value ?? DBNull.Value;
    public virtual async Task<int?> CreateAsync(BusinessEntity business, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Business_Create", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = business.Name });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = DbNullIfNull(business.Email) });
        cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50) { Value = DbNullIfNull(business.Phone) });
        cmd.Parameters.Add(new SqlParameter("@TimeZoneIana", SqlDbType.NVarChar, 100) { Value = DbNullIfNull(business.TimeZoneIana) });
        cmd.Parameters.Add(new SqlParameter("@SettingsJson", SqlDbType.NVarChar, -1) { Value = DbNullIfNull(business.SettingsJson) });

        var outId = new SqlParameter("@BusinessId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(outId);

        await cmd.ExecuteNonQueryAsync(ct);
        return (int?)outId.Value;
    }

    public virtual async Task UpdateAsync(BusinessEntity business, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Business_Update", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = business.BusinessId });
        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = business.Name });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = DbNullIfNull(business.Email) });
        cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50) { Value = DbNullIfNull(business.Phone) });
        cmd.Parameters.Add(new SqlParameter("@TimeZoneIana", SqlDbType.NVarChar, 100) { Value = DbNullIfNull(business.TimeZoneIana) });
        cmd.Parameters.Add(new SqlParameter("@SettingsJson", SqlDbType.NVarChar, -1) { Value = DbNullIfNull(business.SettingsJson) });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public virtual async Task<BusinessEntity?> GetByIdAsync(int businessId, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Business_GetById", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return new BusinessEntity
        {
            BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Email = reader["Email"] as string,
            Phone = reader["Phone"] as string,
            TimeZoneIana = reader.GetString(reader.GetOrdinal("TimeZoneIana")),
            SettingsJson = reader["SettingsJson"] as string,
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
    }

    public virtual async Task DeleteAsync(int businessId, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Business_Delete", cn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

        await cmd.ExecuteNonQueryAsync(ct);
    }
}
