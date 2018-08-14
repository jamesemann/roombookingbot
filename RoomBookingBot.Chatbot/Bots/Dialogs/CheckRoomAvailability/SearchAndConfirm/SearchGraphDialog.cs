using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JamesMann.BotFramework.Middleware;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json.Linq;
using RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability;
using RoomBookingBot.Chatbot.Extensions;
using RoomBookingBot.Chatbot.Model;
using RoomBookingBot.Extensions;
using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
using DateTimePrompt = Microsoft.Bot.Builder.Dialogs.DateTimePrompt;
using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

namespace RoomBookingBot.Chatbot.Bots.Dialogs
{
    public class SearchGraphDialog : DialogContainer
    {
        private SearchGraphDialog() : base(Id)
        {
            Dialogs.Add(Id, new WaterfallStep[]
            {
                 async (dc, args, next) =>
                {
                    var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);
                    stateWrapper.Booking = (BookingRequest)args["bookingRequest"];
                    var bookingEnquiry = stateWrapper.Booking;

                    var rooms = bookingEnquiry.Room == "No preference" ?
                        (from room in bookingEnquiry.AvailableRooms select room.UserPrincipalName) :
                        new string[]{ bookingEnquiry.Room + "@jemann.onmicrosoft.com" }; // see if we can get rid of the string concatenation

                    var meetings = await MicrosoftGraphExtensions.GetMicrosoftGraphFindMeeting(
                        dc.Context.Services.Get<ConversationAuthToken>(AzureAdAuthMiddleware.AUTH_TOKEN_KEY).AccessToken,
                        bookingEnquiry.Start.Value,
                        bookingEnquiry.Start.Value + XmlConvert.ToTimeSpan(bookingEnquiry.MeetingDuration),
                        bookingEnquiry.MeetingDuration,
                        rooms.ToArray());

                    var bookingChoices = new List<(string,string)>();
                    foreach (var suggestion in meetings.MeetingTimeSuggestions)
                    {
                        bookingChoices.AddRange(
                            suggestion.Locations.Select(location=>($"{bookingEnquiry.AvailableRooms.FirstOrDefault(x=>x.UserPrincipalName.ToLower() == location.LocationEmailAddress.ToLower()).DisplayName}: {DateTime.Parse(suggestion.MeetingTimeSlot.Start.DateTime).ToString("hh:mm")} - {DateTime.Parse(suggestion.MeetingTimeSlot.End.DateTime).ToString("hh:mm")}",location.LocationEmailAddress))
                        );
                    }

                    var activity = dc.Context.Activity.CreateReply();
                    activity.AddAdaptiveCardChoiceForm(bookingChoices.ToArray());
                    await dc.Context.SendActivity(activity);
                },
                async(dc,args, next) =>{
                    if (args["Activity"] is Activity activity && activity.Value != null && ((dynamic)activity.Value).chosenRoom is JValue chosenRoom)
                    {
                        await dc.End(new Dictionary<string, object>
                        {
                            ["Value"] = chosenRoom.Value
                        });
                    }
                    else
                    {
                        await dc.Begin(Id, dc.ActiveDialog.State);
                    }
                }
            });

            Dialogs.Add("textPrompt", new TextPrompt());
            Dialogs.Add("choicePrompt", new ChoicePrompt("en"));
        }
        public static string Id => "searchGraphDialog";

        public static SearchGraphDialog Instance = new SearchGraphDialog();
    }

    // convenience helper to get/set dialog state
    public class SearchGraphDialogStateWrapper
    {
        public SearchGraphDialogStateWrapper(IDictionary<string, object> state)
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
