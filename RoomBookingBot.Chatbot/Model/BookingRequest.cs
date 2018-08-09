using System;

namespace RoomBookingBot.Chatbot.Model
{
    public class BookingRequest
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Room { get; set; }

        public override string ToString()
        {
            var result = $"Booking ";
            if (Start.HasValue)
            {
                result += $"from: {Start.Value} ";
            }
            if (End.HasValue)
            {
                result += $"to: {End.Value} ";
            }
            if (!string.IsNullOrEmpty(Room))
            {
                result += $"room: {Room}";
            }
            return result;
        }
    }
}

