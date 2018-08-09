using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Recognizers.Text;
using RoomBookingBot.Chatbot.Extensions;
using RoomBookingBot.Chatbot.Model;
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

                    if (string.IsNullOrEmpty(bookingRequest.Room))
                    {
                        await dc.Prompt("textPrompt", "which room would you like to book?");
                    }
                    else {
                        await dc.Continue();
                    }
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;

                    if (string.IsNullOrEmpty(bookingRequest.Room))
                    {
                        bookingRequest.Room = (string)args["Value"];
                    }

                    if (!bookingRequest.Start.HasValue)
                    {
                        await dc.Prompt("dateTimePrompt", "when would you like to start?");
                    }
                    else
                    {
                        await dc.Continue();
                    }
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;

                    if (!bookingRequest.Start.HasValue)
                    {
                        bookingRequest.Start = (args["Resolution"] as List<DateTimeResolution>).ToDateTime();
                    }

                    if (!bookingRequest.End.HasValue)
                    {
                        await dc.Prompt("dateTimePrompt", "when would you like to end?");
                    }
                    else
                    {
                        await dc.Continue();
                    }
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;

                    if (!bookingRequest.End.HasValue)
                    {
                        bookingRequest.End = (args["Resolution"] as List<DateTimeResolution>).ToDateTime();
                    }
                    await dc.Context.SendActivity($"done {bookingRequest}");
                }
            });

            Dialogs.Add("dateTimePrompt", new DateTimePrompt("en"));
            Dialogs.Add("textPrompt", new TextPrompt());
            Dialogs.Add("numberPrompt", new NumberPrompt<int>(Culture.English));
        }

        public static string Id => "checkRoomAvailabilityDialog";
        public static CheckRoomAvailabilityDialog Instance { get; } = new CheckRoomAvailabilityDialog();
    }
}
