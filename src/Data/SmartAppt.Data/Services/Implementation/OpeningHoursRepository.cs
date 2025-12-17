using Data.SmartAppt.SQL.Configs;
using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation;

public class OpeningHoursRepository : IOpeningHoursRepository
{
    private readonly string? _connectionString;

    public OpeningHoursRepository(IOptions<DataBaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public virtual async Task<OpeningHoursEntity?> GetByDateAsync(int businessId, DateTime dateUtc, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.OpeningHours_GetByDate", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
        cmd.Parameters.Add(new SqlParameter("@DateUtc", SqlDbType.Date) { Value = dateUtc.Date });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return new OpeningHoursEntity
        {
            OpenTimeUtc = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
            CloseTimeUtc = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
        };
    }

    public async Task<List<OpeningHoursEntity>> GetOpeningHoursAsync(int businessId, CancellationToken ct)
    {
        var result = new List<OpeningHoursEntity>();

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.OpeningHours_GetAll", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            result.Add(new OpeningHoursEntity
            {
                OpeningHoursId = reader.GetInt32(reader.GetOrdinal("OpeningHoursId")),
                BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                DayOfWeek = (DayOfWeek)reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                OpenTimeUtc = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
                CloseTimeUtc = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
            });
        }

        return result;
    }
}