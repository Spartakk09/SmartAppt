using Business.SmartAppt.Models.Business;

namespace Business.SmartAppt.Models.Customer;

public class ListOfBookings
{
    public List<BookingModel> Bookings { get; set; } = new List<BookingModel>();
}
