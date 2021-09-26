using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Services
{
    public class ShiftService : IShiftService
    {
        private readonly AuthenticationStateProvider _authenticationState;
        private readonly GraphServiceClient _graphServiceClient;

        public ShiftService(AuthenticationStateProvider authenticationState, GraphServiceClient graphServiceClient)
        {
            _authenticationState = authenticationState;
            _graphServiceClient = graphServiceClient;
        }

        public async Task<IEnumerable<ShiftViewModel>> GetShiftsAsync(string teamId)
        {
            var userId = (await _authenticationState.GetAuthenticationStateAsync())
                .User.Claims.Single(x => x.Type == "oid").Value;

            var openShifts = await _graphServiceClient
                .Teams[teamId]
                .Schedule
                .OpenShifts
                .Request()
                .GetAsync();

            var assignedShifts = await _graphServiceClient
                .Teams[teamId]
                .Schedule
                .Shifts
                .Request()
                .GetAsync();

            return openShifts
                .Select(x => new ShiftViewModel(x, assignedShifts, userId))
                .ToList();
        }

        public async Task BookShiftAsync(string teamId, ShiftViewModel shift)
        {
            var userId = (await _authenticationState.GetAuthenticationStateAsync())
                .User.Claims.Single(x => x.Type == "oid").Value;

            Shift assignment = shift.BuildShift(userId);

            await _graphServiceClient.Teams[teamId].Schedule.Shifts.Request().AddAsync(assignment);

            var calEvent = shift.BuildEvent();

            await _graphServiceClient.Me.Calendar.Events.Request().AddAsync(calEvent);
        }

        public async Task CancelBookingAsync(string teamId, ShiftViewModel shift)
        {
            await _graphServiceClient.Teams[teamId].Schedule.Shifts[shift.AssignmentId].Request().DeleteAsync();

            var source = shift.BuildEvent();
            var calEvent = (await _graphServiceClient.Me
                .Calendar
                .Events
                .Request()
                .Filter($"subject eq 'Black friday shift'")
                .GetAsync())
                .Where(x => x.Start.DateTime == source.Start.DateTime)
                .FirstOrDefault();

            if (calEvent != null)
            {
                await _graphServiceClient.Me
                    .Calendar
                    .Events[calEvent.Id]
                    .Request()
                    .DeleteAsync();
            }
        }
    }
}
