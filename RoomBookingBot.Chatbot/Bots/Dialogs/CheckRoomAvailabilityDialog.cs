using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JamesMann.BotFramework.Middleware;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Graph;
using Microsoft.Recognizers.Text;
using RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability;
using RoomBookingBot.Chatbot.Extensions;
using RoomBookingBot.Chatbot.Model;
using RoomBookingBot.Extensions;
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
                    var bookingRequest = args["bookingRequest"] as BookingRequest;
                    dc.ActiveDialog.State["bookingRequest"] = bookingRequest;
                    await dc.Continue();
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State.ContainsKey("bookingRequest") ? dc.ActiveDialog.State["bookingRequest"] as BookingRequest : args["bookingRequest"] as BookingRequest;

                    if (string.IsNullOrEmpty(bookingRequest.Room))
                    {
                        bookingRequest.AvailableRooms = (await MicrosoftGraphExtensions.GetMicrosoftGraphFindMeetingRooms(
                            dc.Context.Services.Get<ConversationAuthToken>(AzureAdAuthMiddleware.AUTH_TOKEN_KEY).AccessToken) as IList<Person>).ToArray();

                        var roomChoices = new List<Choice>();
                        roomChoices.Add(new Choice {Value = "No preference"});
                        roomChoices.AddRange(from room in bookingRequest.AvailableRooms select new Choice() {Value=room.UserPrincipalName.Split('@')[0] });

                        await dc.Prompt("choice", "Do you have a preference which room?", new ChoicePromptOptions
                        {
                            Choices = roomChoices
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await dc.Continue();
                    }
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    if (string.IsNullOrEmpty(bookingRequest.Room))
                    {
                        bookingRequest.Room = ((FoundChoice) args["Value"]).Value;
                    }

                    await dc.Continue();
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    // TODO rewrite to use custom dialog
                    await (!bookingRequest.Start.HasValue ? dc.Prompt("dateTimePrompt", "When would you like your meeting?") : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    if (!bookingRequest.Start.HasValue)
                    {
                        (bookingRequest.Start, _) = (args["Resolution"] as List<DateTimeResult.DateTimeResolution>).ToDateTime();
                    }

                    await dc.Continue();
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    await (!bookingRequest.RequestedStartTimeIsValid() ? dc.Begin(DisambiguateTimeDialog.Id) : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    if (!bookingRequest.RequestedStartTimeIsValid())
                    {
                        bookingRequest.Start += (TimeSpan) args["time"];
                    }

                    await dc.Continue();
                },
                async (dc, args, next) =>
                {
                    var bookingRequest = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;
                    await (string.IsNullOrEmpty(bookingRequest.MeetingDuration) ? dc.Prompt("dateTimePrompt", "How long do you need the room for?") : dc.Continue());
                },
                async (dc, args, next) =>
                {
                    var bookingEnquiry = dc.ActiveDialog.State["bookingRequest"] as BookingRequest;

                    if (string.IsNullOrEmpty(bookingEnquiry.MeetingDuration))
                    {
                        var duration = (args["Resolution"] as List<DateTimeResult.DateTimeResolution>).ToTimex();
                        bookingEnquiry.MeetingDuration = duration;
                    }

                    // first do a precise search

                    // see if we can get rid of the string concatenation
                    var rooms = bookingEnquiry.Room == "No preference" ? (from room in bookingEnquiry.AvailableRooms select room.UserPrincipalName) : new string[]{ bookingEnquiry.Room + "@jemann.onmicrosoft.com" };

                    var meetings = await MicrosoftGraphExtensions.GetMicrosoftGraphFindMeeting(
                        dc.Context.Services.Get<ConversationAuthToken>(AzureAdAuthMiddleware.AUTH_TOKEN_KEY).AccessToken,
                        bookingEnquiry.Start.Value,
                        bookingEnquiry.Start.Value + XmlConvert.ToTimeSpan(bookingEnquiry.MeetingDuration),
                        bookingEnquiry.MeetingDuration,
                        rooms.ToArray());


                    var bookingChoices = new List<Choice>();
                    foreach (var suggestion in meetings.MeetingTimeSuggestions)
                    {
                        foreach(var location in suggestion.Locations){
                            bookingChoices.Add(new Choice() { Value = $"{location.LocationEmailAddress} ({suggestion.MeetingTimeSlot.Start.DateTime}-{suggestion.MeetingTimeSlot.Start.DateTime})" } );
                        }
                        //suggestion.MeetingTimeSlot.
                    }

                    if (bookingChoices.Count == 0)
                    {
                        await dc.Context.SendActivity("didnt find any meetings for that timeslot so widening the search...");
                                            var widermeetings = await MicrosoftGraphExtensions.GetMicrosoftGraphFindMeeting(
                        dc.Context.Services.Get<ConversationAuthToken>(AzureAdAuthMiddleware.AUTH_TOKEN_KEY).AccessToken,
                        bookingEnquiry.Start.Value  - TimeSpan.FromHours(12)                       ,
                        bookingEnquiry.Start.Value + XmlConvert.ToTimeSpan(bookingEnquiry.MeetingDuration) + TimeSpan.FromHours(12),
                        bookingEnquiry.MeetingDuration,
                        rooms.ToArray());

                                            foreach (var suggestion in widermeetings.MeetingTimeSuggestions)
                    {
                        foreach(var location in suggestion.Locations){
                            bookingChoices.Add(new Choice() { Value = $"{location.LocationEmailAddress} ({suggestion.MeetingTimeSlot.Start.DateTime}-{suggestion.MeetingTimeSlot.Start.DateTime})" } );
                        }
                        //suggestion.MeetingTimeSlot.
                    }
                    }


                    await dc.Prompt("choice", "Which one would you like", new ChoicePromptOptions
                    {
                        Choices = bookingChoices
                    }).ConfigureAwait(false);


                    // then a wider search if needed (for same day?)


                    //var confirmation = dc.Context.Activity.CreateReply();
                    //confirmation.AddAdaptiveCardRoomConfirmationAttachment(
                    //    bookingEnquiry.Room,
                    //    bookingEnquiry.Start.Value.ToString("dd MMMM"),
                    //    $"{bookingEnquiry.Start.Value.ToString("hh:mm tt")} - {bookingEnquiry.End.Value.ToString("hh:mm tt")} ({(bookingEnquiry.End.Value-bookingEnquiry.Start.Value).TotalMinutes} minutes)",
                    //    "James Mann");
                    //await dc.Context.SendActivity(confirmation);
                },
                async (dc, args, next) =>
                {
                    await dc.End();
                },
            });

            Dialogs.Add("dateTimePrompt", new DateTimePrompt("en"));
            Dialogs.Add("textPrompt", new TextPrompt());
            Dialogs.Add("numberPrompt", new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(Culture.English));
            Dialogs.Add("choice", new ChoicePrompt(Culture.English));

            Dialogs.Add(DisambiguateTimeDialog.Id, DisambiguateTimeDialog.Instance);
        }

        public static string Id => "checkRoomAvailabilityDialog";
        public static CheckRoomAvailabilityDialog Instance { get; } = new CheckRoomAvailabilityDialog();
    }
}