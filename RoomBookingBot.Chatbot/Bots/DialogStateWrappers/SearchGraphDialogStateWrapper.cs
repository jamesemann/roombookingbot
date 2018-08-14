using System.Collections.Generic;
using RoomBookingBot.Chatbot.Model;

namespace RoomBookingBot.Chatbot.Bots.DialogStateWrappers
{
    public class SearchGraphDialogStateWrapper
    {
        public SearchGraphDialogStateWrapper(IDictionary<string, object> state)
        {
            State = state;
        }

        public IDictionary<string, object> State { get; }

        public BookingRequest Booking
        {
            get => State["bookingRequest"] as BookingRequest;
            set => State["bookingRequest"] = value;
        }
    }
}