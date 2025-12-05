using Data.SmartAppt.SQL.Configs;
using Data.SmartAppt.SQL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation;

public class BookingRepository : IBookingRepository
{
    private readonly string? _connectionString;

    public BookingRepository(IOptions<DataBaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task<bool> CancelBookingAsync(int bookingId, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Booking_Delete", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });

        var rowsAffectedParam = new SqlParameter("@RowsAffected", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(rowsAffectedParam);

        await cmd.ExecuteNonQueryAsync(ct);

        return (int)rowsAffectedParam.Value > 0;
    }

    public virtual async Task<List<BookingEntity?>> GetMyBookingsAsync(int customerId, CancellationToken ct)
    {
        var bookings = new List<BookingEntity>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("core.GetMyBookings", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = customerId });

        await connection.OpenAsync(ct);

        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var booking = new BookingEntity
            {
                BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                StartAtUtc = reader.GetDateTime(reader.GetOrdinal("StartAtUtc")),
                EndAtUtc = reader.GetDateTime(reader.GetOrdinal("EndAtUtc")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
            };

            bookings.Add(booking);
        }

        return bookings;
    }

    public async Task<int?> MakeBookingAsync(BookingEntity booking, int durationMin, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Booking_Make", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = booking.BusinessId });
        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = booking.CustomerId });
        cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = booking.ServiceId });
        cmd.Parameters.Add(new SqlParameter("@StartAtUtc", SqlDbType.DateTime2, 3) { Value = booking.StartAtUtc });
        cmd.Parameters.Add(new SqlParameter("@DurationMin", SqlDbType.Int) { Value = durationMin });
        cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 500) { Value = (object?)booking.Notes ?? DBNull.Value });

        var outputId = new SqlParameter("@BookingId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(outputId);

        await cmd.ExecuteNonQueryAsync(ct);

        return outputId.Value == DBNull.Value ? null : (int?)outputId.Value;
    }

    public virtual async Task<int?> UpdateBookingAsync(int oldBookingId, BookingEntity booking, int durationMin, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Booking_Update", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@OldBookingId", SqlDbType.Int) { Value = oldBookingId });
        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = booking.BusinessId });
        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = booking.CustomerId });
        cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = booking.ServiceId });
        cmd.Parameters.Add(new SqlParameter("@StartAtUtc", SqlDbType.DateTime2, 3) { Value = booking.StartAtUtc });
        cmd.Parameters.Add(new SqlParameter("@DurationMin", SqlDbType.Int) { Value = durationMin });
        cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 500) { Value = (object?)booking.Notes ?? DBNull.Value });

        var outputId = new SqlParameter("@NewBookingId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(outputId);

        await cmd.ExecuteNonQueryAsync(ct);

        return outputId.Value == DBNull.Value ? null : (int?)outputId.Value;
    }

    public virtual async Task<BookingEntity?> GetByIdAsync(int bookingId, CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Booking_GetById", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return new BookingEntity
        {
            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
            BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
            StartAtUtc = reader.GetDateTime(reader.GetOrdinal("StartAtUtc")),
            EndAtUtc = reader.GetDateTime(reader.GetOrdinal("EndAtUtc")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
        };
    }

    public virtual async Task<List<GetFreeSlotModel>> GetBookingsForDayAsync(
       int businessId, int serviceId, DateTime dateUtc, CancellationToken ct)
    {
        List<GetFreeSlotModel> bookedSlots = new List<GetFreeSlotModel>();

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Booking_GetByDay", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
        cmd.Parameters.Add(new SqlParameter("@DateUtc", SqlDbType.DateTime2) { Value = dateUtc });

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            bookedSlots.Add(new GetFreeSlotModel
            {
                Start = reader.GetDateTime("StartAtUtc"),
                End = reader.GetDateTime("EndAtUtc")
            }
            );
        }

        return bookedSlots;
    }

    public virtual async Task<List<BookingWithCustomerDetailsModel>>
    GetDailyBookingsWithCustomersAsync(
        int businessId,
        DateTime dateUtc,
        CancellationToken ct)
    {
        var result = new List<BookingWithCustomerDetailsModel>();

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Booking_GetDailyWithCustomers", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
        cmd.Parameters.Add(new SqlParameter("@DateUtc", SqlDbType.Date) { Value = dateUtc.Date });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            result.Add(new BookingWithCustomerDetailsModel
            {
                BookingId = reader.GetInt32("BookingId"),
                StartAtUtc = reader.GetDateTime("StartAtUtc"),
                EndAtUtc = reader.GetDateTime("EndAtUtc"),
                Status = reader.GetString("Status"),
                Notes = reader.GetString("Notes"),

                ServiceId = reader.GetInt32("ServiceId"),
                ServiceName = reader.GetString("ServiceName"),
                DurationMin = reader.GetInt32("DurationMin"),
                Price = reader.GetDecimal("Price"),

                CustomerId = reader.GetInt32("CustomerId"),
                FullName = reader.GetString("FullName"),
                Email = reader.GetString("Email"),
                Phone = reader.GetString("Phone"),
            });
        }

        return result;
    }

    public virtual async Task<List<BookingEntity>> GetAllBookingsByBusinessIdAsync(
    int businessId,
    CancellationToken ct)
    {
        var result = new List<BookingEntity>();

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Booking_GetAllByBusiness", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int)
        {
            Value = businessId
        });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            result.Add(new BookingEntity
            {
                BookingId = reader.GetInt32("BookingId"),
                BusinessId = reader.GetInt32("BusinessId"),
                ServiceId = reader.GetInt32("ServiceId"),
                CustomerId = reader.GetInt32("CustomerId"),
                StartAtUtc = reader.GetDateTime("StartAtUtc"),
                EndAtUtc = reader.GetDateTime("EndAtUtc"),
                Status = reader.GetString("Status"),
                Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                CreatedAtUtc = reader.GetDateTime("CreatedAtUtc")
            });
        }

        return result;
    }

    public virtual async Task<bool> DecideBookingStatusAsync(
    int bookingId,
    string status,
    CancellationToken ct)
    {
        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.Booking_DecideStatus", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int)
        {
            Value = bookingId
        });

        cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar, 12)
        {
            Value = status
        });

        int affectedRows = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);

        return affectedRows > 0;
    }

    public virtual async Task<List<BookingCountByDayModel>> GetMonthlyCalendarAsync(
        int businessId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct)
    {
        var result = new List<BookingCountByDayModel>();

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand("core.GetMonthlyCalendar", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
        cmd.Parameters.Add(new SqlParameter("@FromUtc", SqlDbType.DateTime2) { Value = fromUtc });
        cmd.Parameters.Add(new SqlParameter("@ToUtc", SqlDbType.DateTime2) { Value = toUtc });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            result.Add(new BookingCountByDayModel
            {
                BookingDate = reader.GetDateTime(reader.GetOrdinal("BookingDate")),
                BookingCount = reader.GetInt32(reader.GetOrdinal("BookingCount"))
            });
        }

        return result;
    }
}