using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Services
{
    public interface IShiftService
    {
        Task<IEnumerable<ShiftViewModel>> GetShiftsAsync(string teamId);

        Task BookShiftAsync(string teamId, ShiftViewModel shift);

        Task CancelBookingAsync(string teamId, ShiftViewModel shift);
    }
}
