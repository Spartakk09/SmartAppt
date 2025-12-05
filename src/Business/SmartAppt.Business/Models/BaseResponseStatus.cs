using System.Text.Json.Serialization;

namespace Business.SmartAppt.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BaseResponseStatus
{
    Success,
    NotFound,
    ValidationError,
    DatabaseError,
    UnknownError,
    IsValid,
    CreationFailed,
    AlreadyExists,
    InvalidBusiness,
    InvalidService,
    InvalidCustomer,
    InvalidBooking,
    Holiday,
    NoWorkingHours,
    AlreadyCanceled,
    Empty,
    Busy,
    FailToUpdate,
    InvalidStatus
}
