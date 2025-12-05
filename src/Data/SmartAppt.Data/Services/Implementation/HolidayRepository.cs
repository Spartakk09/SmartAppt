
using Data.SmartAppt.SQL.Configs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

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
}
