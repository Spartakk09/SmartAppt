using Data.SmartAppt.SQL.Configs;
using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation;

public class CustomerRepository : ICustomerRepository
{
    private readonly string? _connectionString;
    public CustomerRepository(IOptions<DataBaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    private static object DbNullIfNull(object? value) => value ?? DBNull.Value;
    public virtual async Task<int?> CreateAsync(CustomerEntity customer, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Customer_Create", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = customer.BusinessId });
        cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 200) { Value = customer.FullName });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = DbNullIfNull(customer.Email) });
        cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50) { Value = DbNullIfNull(customer.Phone) });

        var outId = new SqlParameter("@CustomerId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(outId);

        await cmd.ExecuteNonQueryAsync(ct);

        return (int?)outId.Value;
    }

    public virtual async Task DeleteAsync(int customerId, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Customer_Delete", cn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = customerId });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public virtual async Task<CustomerEntity?> GetByIdAsync(int customerId, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Customer_GetById", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = customerId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return new CustomerEntity
        {
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Email = reader["Email"] as string,
            Phone = reader["Phone"] as string,
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
    }

    public virtual async Task UpdateAsync(CustomerEntity customer, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Customer_Update", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = customer.BusinessId });
        cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 200) { Value = customer.FullName });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = DbNullIfNull(customer.Email) });
        cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50) { Value = DbNullIfNull(customer.Phone) });
        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = customer.CustomerId });

        await cmd.ExecuteNonQueryAsync(ct);
    }
}