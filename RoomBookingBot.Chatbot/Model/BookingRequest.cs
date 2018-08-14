using Microsoft.Graph;
using System;

namespace RoomBookingBot.Chatbot.Model
{
    public class BookingRequest
    {
        public DateTime? Start { get; set; }
        //public DateTime? End { get; set; }
        public string Room { get; set; }

        public bool RequestedStartTimeIsValid()
        {
            return Start.HasValue && Start.Value.TimeOfDay.Hours >= 9;
        }
        public string MeetingDuration { get; set; }

        public Person[] AvailableRooms { get; set; }

        public override string ToString()
        {
            var result = $"Booking ";
            if (Start.HasValue)
            {
                result += $"from: {Start.Value} ";
            }
            if (!string.IsNullOrWhiteSpace(MeetingDuration))
            {
                result += $"to: {MeetingDuration} ";
            }
            if (!string.IsNullOrEmpty(Room))
            {
                result += $"room: {Room}";
            }
            return result;
        }
    }
}

