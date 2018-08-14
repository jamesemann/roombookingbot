using System.Collections.Generic;
using RoomBookingBot.Chatbot.Model;

namespace RoomBookingBot.Chatbot.Bots.DialogStateWrappers
{
    public class CheckRoomAvailabilityDialogStateWrapper
    {
        public CheckRoomAvailabilityDialogStateWrapper(IDictionary<string, object> state)
        {
            State = state;
        }

        public IDictionary<string, object> State { get; }

        public BookingRequest Booking
        {
            get => (BookingRequest) State["bookingRequest"];
            set => State["bookingRequest"] = value;
        }
    }
}