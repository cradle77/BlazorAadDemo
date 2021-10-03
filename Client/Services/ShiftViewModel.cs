using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Services
{
    public class ShiftViewModel
    {
        public string Id { get; set; }
        public string AssignmentId { get; private set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string SchedulingGroupId { get; }

        public DateTime Date 
        {
            get { return this.From.Date; }
        }

        public string TimeRange
        {
            get 
            {
                return string.Format("{0:t} - {1:t}", this.From, this.To);
            }
        }

        public ShiftViewModel(OpenShift source, IEnumerable<Shift> assignedShifts)
        {
            this.From = source.SharedOpenShift.StartDateTime.Value.DateTime;
            this.To = source.SharedOpenShift.EndDateTime.Value.DateTime;

            this.SchedulingGroupId = source.SchedulingGroupId;

            var assignedShift = assignedShifts.SingleOrDefault(assigned =>
                assigned.SharedShift.StartDateTime == source.SharedOpenShift.StartDateTime &&
                assigned.SharedShift.EndDateTime == source.SharedOpenShift.EndDateTime);

            this.Id = source.Id;
            this.AssignmentId = assignedShift?.Id;
            this.IsAssigned = source.SharedOpenShift.OpenSlotCount == 0;
            this.IsMine = assignedShift != null;

            this.OpenShift = source;
        }

        public bool IsAssigned { get; set; }

        public bool IsMine { get; set; }
        public OpenShift OpenShift { get; private set; }

        internal Shift BuildShift(string currentUserId)
        {
            return new Shift()
            {
                UserId = currentUserId,
                SchedulingGroupId = this.SchedulingGroupId,
                SharedShift = new ShiftItem
                {
                    StartDateTime = new DateTimeOffset(this.From),
                    EndDateTime = new DateTimeOffset(this.To),
                    Theme = ScheduleEntityTheme.Blue
                }
            };
        }

        internal Event BuildEvent()
        {
            return new Event()
            {
                Subject = "Black friday shift",
                Start = DateTimeTimeZone.FromDateTime(this.From, "Etc/GMT"),
                End = DateTimeTimeZone.FromDateTime(this.To, "Etc/GMT"),
                ShowAs = FreeBusyStatus.Busy,
                TransactionId = this.Id
            };
        }
    }
}
