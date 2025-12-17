using Data.SmartAppt.SQL.Configs;
using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation;

public class ServiceRepository : IServiceRepository
{
    private readonly string? _connectionString;

    public ServiceRepository(IOptions<DataBaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public virtual async Task<int?> CreateAsync(ServiceEntity service, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Service_Create", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = service.BusinessId });
        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = service.Name });
        cmd.Parameters.Add(new SqlParameter("@DurationMin", SqlDbType.Int) { Value = service.DurationMin });

        var priceParam = new SqlParameter("@Price", SqlDbType.Decimal)
        {
            Precision = 10,
            Scale = 2,
            Value = service.Price
        };
        cmd.Parameters.Add(priceParam);

        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = service.IsActive });

        var outId = new SqlParameter("@ServiceId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(outId);

        await cmd.ExecuteNonQueryAsync(ct);

        return (int?)outId.Value;
    }

    public virtual async Task<ServiceEntity?> GetByIdAsync(int serviceId, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Service_GetById", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return new ServiceEntity
        {
            ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
            BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            DurationMin = reader.GetInt32(reader.GetOrdinal("DurationMin")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    public virtual async Task UpdateAsync(ServiceEntity service, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Service_Update", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = service.ServiceId });
        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = service.BusinessId });
        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = service.Name });
        cmd.Parameters.Add(new SqlParameter("@DurationMin", SqlDbType.Int) { Value = service.DurationMin });

        var priceParam = new SqlParameter("@Price", SqlDbType.Decimal)
        {
            Precision = 10,
            Scale = 2,
            Value = service.Price
        };
        cmd.Parameters.Add(priceParam);

        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = service.IsActive });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public virtual async Task DeleteAsync(int serviceId, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Service_Delete", cn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public virtual async Task DisableServiceAsync(int serviceId, CancellationToken ct)
    {

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Service_Disable", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public virtual async Task<List<ServiceEntity>> GetSeviceByBusinessIdAsync(int businessId, int skip = 0, int take = 10, CancellationToken ct = default)
    {
        var list = new List<ServiceEntity>();

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Service_GetByBusinessId", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
        cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
        cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            list.Add(new ServiceEntity
            {
                ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                DurationMin = reader.GetInt32(reader.GetOrdinal("DurationMin")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            });
        }

        return list;
    }
}
