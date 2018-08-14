using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Recognizers.Text;
using RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability;
using RoomBookingBot.Chatbot.Extensions;
using RoomBookingBot.Chatbot.Model;
using System;
using System.Collections.Generic;
using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
using DateTimePrompt = Microsoft.Bot.Builder.Dialogs.DateTimePrompt;
using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

namespace RoomBookingBot.Chatbot.Bots.Dialogs
{
    public class CheckRoomAvailabilityDialog : DialogContainer
    {
        public CheckRoomAvailabilityDialog() : base(Id)
        {
            Dialogs.Add(Id, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);
                    var bookingRequest = args["bookingRequest"] as BookingRequest;
                    stateWrapper.Booking = bookingRequest;
                    await dc.Begin(DisambiguateRoomDialog.Id, dc.ActiveDialog.State);// : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);
                    var bookingRequest = stateWrapper.Booking;
                    if (string.IsNullOrEmpty(bookingRequest.Room)){
                        bookingRequest.Room = (string)args["Value"];
                    }

                    await (!bookingRequest.Start.HasValue ? dc.Begin(DisambiguateDateDialog.Id) : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);
                    var bookingRequest = stateWrapper.Booking;
                    if (!bookingRequest.Start.HasValue)
                    {
                        bookingRequest.Start = (DateTime) args["date"];
                    }
                    
                    await (!bookingRequest.RequestedStartTimeIsValid() ? dc.Begin(DisambiguateTimeDialog.Id) : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);
                    var bookingRequest = stateWrapper.Booking;
                    if (!bookingRequest.RequestedStartTimeIsValid())
                    {
                        bookingRequest.Start += (TimeSpan) args["time"];
                    }

                    await (string.IsNullOrEmpty(bookingRequest.MeetingDuration) ? dc.Prompt("dateTimePrompt", "How long do you need the room for?") : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);
                    var bookingEnquiry = stateWrapper.Booking;

                    if (string.IsNullOrEmpty(bookingEnquiry.MeetingDuration))
                    {
                        var duration = (args["Resolution"] as List<DateTimeResult.DateTimeResolution>).ToTimex();
                        bookingEnquiry.MeetingDuration = duration;
                    }

                    await dc.Begin(SearchGraphDialog.Id, dc.ActiveDialog.State);
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);
                    stateWrapper.Booking = null;

                    await dc.End();
                }
            });

            Dialogs.Add("dateTimePrompt", new DateTimePrompt("en"));
            Dialogs.Add("textPrompt", new TextPrompt());
            Dialogs.Add("numberPrompt", new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(Culture.English));
            Dialogs.Add("choice", new ChoicePrompt(Culture.English));

            Dialogs.Add(DisambiguateTimeDialog.Id, DisambiguateTimeDialog.Instance);
            Dialogs.Add(DisambiguateDateDialog.Id, DisambiguateDateDialog.Instance);
            Dialogs.Add(DisambiguateRoomDialog.Id, DisambiguateRoomDialog.Instance);
            Dialogs.Add(SearchGraphDialog.Id, SearchGraphDialog.Instance);
        }

        public static string Id => "checkRoomAvailabilityDialog";
        public static CheckRoomAvailabilityDialog Instance { get; } = new CheckRoomAvailabilityDialog();
    }

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