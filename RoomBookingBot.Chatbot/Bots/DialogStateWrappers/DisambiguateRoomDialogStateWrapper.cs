using RoomBookingBot.Chatbot.Model;
using System.Collections.Generic;

namespace RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability
{
    public class DisambiguateRoomDialogStateWrapper
    {
        public DisambiguateRoomDialogStateWrapper(IDictionary<string, object> state)
        {
            State = state;
        }

        public IDictionary<string, object> State { get; }

        public BookingRequest Booking
        {
            get
            {
                return State["bookingRequest"] as BookingRequest;
            }
            set
            {
                State["bookingRequest"] = value;
            }
        }

    }
}
