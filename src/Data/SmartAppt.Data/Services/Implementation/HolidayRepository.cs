using Data.SmartAppt.SQL.Configs;
using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation;

public class HolidayRepository : IHolidayRepository
{
    private readonly string? _connectionString;

    public HolidayRepository(IOptions<DataBaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }
    public virtual async Task<bool> ExistsAsync(int businessId, DateTime dateUtc, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Holiday_Exists", cn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", System.Data.SqlDbType.Int) { Value = businessId });
        cmd.Parameters.Add(new SqlParameter("@DateUtc", System.Data.SqlDbType.Date) { Value = dateUtc.Date });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result != null;
    }

    public virtual async Task<List<HolidayEntity>> GetInRangeAsync(
            int businessId,
            DateTime start,
            DateTime end,
            CancellationToken ct)
    {
        var result = new List<HolidayEntity>();

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Holiday_GetInRange", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
        cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = start.Date });
        cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = end.Date });

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new HolidayEntity
            {
                HolidayId = reader.GetInt32("HolidayId"),
                BusinessId = reader.GetInt32("BusinessId"),
                HolidayDate = reader.GetDateTime("HolidayDate"),
                Reason = reader.GetString("Reason")
            });
        }

        return result;
    }
}
