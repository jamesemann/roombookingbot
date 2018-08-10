using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using RoomBookingBot.Chatbot.Extensions;
using RoomBookingBot.Chatbot.Model;
using System;
using System.Collections.Generic;
using static Microsoft.Bot.Builder.Prompts.DateTimeResult;

namespace RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability
{
    public class CheckRoomAvailabilityDialog : DialogContainer
    {
        public CheckRoomAvailabilityDialog() : base(Id)
        {
            Dialogs.Add(Id, new WaterfallStep[]
            {
                async (dc, args, next) => {
                    var bookingRequest = args["bookingRequest"] as BookingRequest;
                    dc.ActiveDialog.State["bookingRequest"] = bookingRequest;

                    // dialog has to either Continue, Prompt, or End, Begin, EndAll, Replace
                    await dc.Continue();
                },
                async (dc, args, next) => {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    await (string.IsNullOrEmpty(bookingRequest.Room) ? dc.Prompt("textPrompt", "Which room would you like to book?") : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    if (string.IsNullOrEmpty(bookingRequest.Room))
                    {
                        bookingRequest.Room = (string)args["Value"];
                    }
                    await dc.Continue();
                },
                async (dc, args, next) =>
                {   
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    await (!bookingRequest.Start.HasValue ? dc.Prompt("dateTimePrompt", "When is your meeting?") : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    if (!bookingRequest.Start.HasValue)
                    {
                        (bookingRequest.Start, bookingRequest.StartContainsTimePart) = (args["Resolution"] as List<DateTimeResolution>).ToDateTime();
                    }
                    await dc.Continue();
                },
                async (dc, args, next) =>
                {   
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    await (!bookingRequest.StartContainsTimePart ? dc.Begin(DisambiguateTimeDialog.Id) : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    if (!bookingRequest.StartContainsTimePart)
                    {
                        bookingRequest.Start += ((TimeSpan)args["time"]);
                    }
                    await dc.Continue();
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    await (!bookingRequest.End.HasValue ? dc.Prompt("dateTimePrompt", "How long do you need the room for?") : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = (dc.ActiveDialog.State["bookingRequest"] as BookingRequest);

                    if (!bookingRequest.End.HasValue)
                    {
                        var dateTime = (args["Resolution"] as List<DateTimeResolution>).ToDateTime().parsedDate;
                        if (dateTime > System.DateTime.MinValue)
                        {
                            bookingRequest.End = dateTime;
                        }
                        else
                        {
                            var duration = (args["Resolution"] as List<DateTimeResolution>).ToTimeSpan();
                            bookingRequest.End = bookingRequest.Start + duration;
                        }
                    }
                    await dc.Context.SendActivity($"done {bookingRequest}");
                }
            });

            Dialogs.Add("dateTimePrompt", new DateTimePrompt("en"));
            Dialogs.Add("textPrompt", new TextPrompt());
            Dialogs.Add("numberPrompt", new NumberPrompt<int>(Culture.English));

            Dialogs.Add(DisambiguateTimeDialog.Id, DisambiguateTimeDialog.Instance);
        }

        public static string Id => "checkRoomAvailabilityDialog";
        public static CheckRoomAvailabilityDialog Instance { get; } = new CheckRoomAvailabilityDialog();
    }
}
