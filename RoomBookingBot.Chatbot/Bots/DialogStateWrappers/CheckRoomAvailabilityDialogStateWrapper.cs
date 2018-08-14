using RoomBookingBot.Chatbot.Model;
using System.Collections.Generic;

namespace RoomBookingBot.Chatbot.Bots.Dialogs
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
            get
            {
                return (BookingRequest)State["bookingRequest"];
            }
            set
            {
                State["bookingRequest"] = value;
            }
        }
    }
}