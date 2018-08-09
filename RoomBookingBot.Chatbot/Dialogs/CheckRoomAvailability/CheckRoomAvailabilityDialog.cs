using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RoomBookingBot.Luis.Extensions.LuisExtensions;

namespace RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability
{
    // TODO this dialog needs to be able to disambiguate the request...
    public class CheckRoomAvailabilityDialog : DialogContainer
    {
        public CheckRoomAvailabilityDialog() : base(Id)
        {
            Dialogs.Add(Id, new WaterfallStep[]
            {
                async (dc, args, next) => {
                    var bookingRequest = args["bookingRequest"] as BookingRequest;
                    dc.ActiveDialog.State["bookingRequest"] = bookingRequest;

                    // disambiguation
                    await Disambiguate(dc, bookingRequest);

                    // if we get to here, end
                    //await dc.End();
                    //await dc.Prompt("textPrompt", "Who would you like to pay?");
                },
                async (dc, args, next) =>
                {
                    // save the room if required
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;

                    if (string.IsNullOrEmpty(bookingRequest.Room))
                    {
                        bookingRequest.Room = (string)args["Value"];
                        await Disambiguate(dc, bookingRequest);
                    } else
                    {
                        await dc.Continue();
                    }
                },
                
                async (dc, args, next) =>
                {
                    // save the room if required
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;

                    if (!bookingRequest.Start.HasValue)
                    {
                        // TODO prob needs some checking here...
                        bookingRequest.Start = DateTime.Parse(((List<Microsoft.Bot.Builder.Prompts.DateTimeResult.DateTimeResolution>)args["Resolution"]).FirstOrDefault().Value);
                        await Disambiguate(dc, bookingRequest);
                    } else
                    {
                        await dc.Continue();
                    }

                    
                },
                async (dc, args, next) =>
                {
                    // save the room if required
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;

                    if (!bookingRequest.End.HasValue)
                    {
                        // TODO prob needs some checking here...
                        bookingRequest.End = DateTime.Parse(((List<Microsoft.Bot.Builder.Prompts.DateTimeResult.DateTimeResolution>)args["Resolution"]).FirstOrDefault().Value);
                        await Disambiguate(dc, bookingRequest);
                    } else
                    {
                        await dc.Continue();
                    }
                    await dc.Context.SendActivity($"done  {bookingRequest}");

                }
            });

            // add the prompts
            Dialogs.Add("dateTimePrompt", new DateTimePrompt("en"));
            Dialogs.Add("textPrompt", new TextPrompt());
            Dialogs.Add("numberPrompt", new NumberPrompt<int>(Culture.English));
        }

        public async Task Disambiguate(DialogContext dc, BookingRequest bookingRequest)
        {
            if (string.IsNullOrEmpty(bookingRequest.Room))
            {
                // TODO replace with choiceprompt
                await dc.Prompt("textPrompt", "which room would you like to book?");
                return;
            }
            else if (!bookingRequest.Start.HasValue)
            {
                // TODO replace with dateprompt
                await dc.Prompt("dateTimePrompt", "when would you like to start?");
                return;
            }
            else if (!bookingRequest.End.HasValue)
            {
                // TODO replace with dateprompt
                await dc.Prompt("dateTimePrompt", "when would you like to end?");
                return;
            }
        }

        public static string Id => "checkRoomAvailabilityDialog";
        public static CheckRoomAvailabilityDialog Instance { get; } = new CheckRoomAvailabilityDialog();
    }
}
