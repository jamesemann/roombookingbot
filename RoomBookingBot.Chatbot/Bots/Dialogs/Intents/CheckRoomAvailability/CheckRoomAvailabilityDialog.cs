using System;
using System.Collections.Generic;
using JamesMann.BotFramework.Dialogs.Date;
using JamesMann.BotFramework.Dialogs.Time;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using RoomBookingBot.Chatbot.Bots.Dialogs.Intents.CheckRoomAvailability.DisambiguateRoom;
using RoomBookingBot.Chatbot.Bots.Dialogs.Intents.CheckRoomAvailability.SearchAndConfirm;
using RoomBookingBot.Chatbot.Bots.DialogStateWrappers;
using RoomBookingBot.Chatbot.Extensions;
using RoomBookingBot.Chatbot.Model;
using DateTimePrompt = Microsoft.Bot.Builder.Dialogs.DateTimePrompt;

namespace RoomBookingBot.Chatbot.Bots.Dialogs.Intents.CheckRoomAvailability
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
                    await dc.Begin(DisambiguateRoomDialog.Id, dc.ActiveDialog.State); 
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);
                    var bookingRequest = stateWrapper.Booking;
                    if (string.IsNullOrEmpty(bookingRequest.Room))
                    {
                        bookingRequest.Room = (string) args["Value"];
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
                }
            });

            Dialogs.Add("dateTimePrompt", new DateTimePrompt("en"));
            Dialogs.Add(DisambiguateTimeDialog.Id, DisambiguateTimeDialog.Instance);
            Dialogs.Add(DisambiguateDateDialog.Id, DisambiguateDateDialog.Instance);
            Dialogs.Add(DisambiguateRoomDialog.Id, DisambiguateRoomDialog.Instance);
            Dialogs.Add(SearchGraphDialog.Id, SearchGraphDialog.Instance);
        }

        public static string Id => "checkRoomAvailabilityDialog";
        public static CheckRoomAvailabilityDialog Instance { get; } = new CheckRoomAvailabilityDialog();
    }
}