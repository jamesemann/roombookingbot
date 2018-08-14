using JamesMann.BotFramework.Middleware;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoomBookingBot.Chatbot.Extensions;
using RoomBookingBot.Chatbot.Model;
using RoomBookingBot.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml;

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
                        new string[]{ bookingEnquiry.AvailableRooms.FirstOrDefault(x=>x.DisplayName == bookingEnquiry.Room).UserPrincipalName };

                    var meetings = await MicrosoftGraphExtensions.GetMicrosoftGraphFindMeeting(
                        dc.Context.Services.Get<ConversationAuthToken>(AzureAdAuthMiddleware.AUTH_TOKEN_KEY).AccessToken,
                        bookingEnquiry.Start.Value,
                        bookingEnquiry.Start.Value + XmlConvert.ToTimeSpan(bookingEnquiry.MeetingDuration),
                        bookingEnquiry.MeetingDuration,
                        rooms.ToArray());

                    var bookingChoices = new List<(string,object)>();
                    foreach (var suggestion in meetings.MeetingTimeSuggestions)
                    {
                        foreach(var location in suggestion.Locations){
                        var display = $"{bookingEnquiry.AvailableRooms.FirstOrDefault(x=>x.UserPrincipalName.ToLower() == location.LocationEmailAddress.ToLower()).DisplayName}: {DateTime.Parse(suggestion.MeetingTimeSlot.Start.DateTime).ToString("hh:mm")} - {DateTime.Parse(suggestion.MeetingTimeSlot.End.DateTime).ToString("hh:mm")}";
                        var value = new { start = DateTime.Parse(suggestion.MeetingTimeSlot.Start.DateTime), end = DateTime.Parse(suggestion.MeetingTimeSlot.End.DateTime),  roomEmail = location.LocationEmailAddress };
                        bookingChoices.Add((display,JsonConvert.SerializeObject( value )));
                            }
                    }

                    if(bookingChoices.Count == 0)
                    {
                        await dc.Context.SendActivity(dc.Context.Activity.CreateReply("Couldn't find any availability for that timeslot. A future improvement might be to widen the search by location or timeslots"));
                        dc.EndAll();
                    }
                    else {
                    var activity = dc.Context.Activity.CreateReply();
                    activity.AddAdaptiveCardChoiceForm(bookingChoices.ToArray());
                    await dc.Context.SendActivity(activity);
                    }
                },
                async(dc,args, next) =>{
                    if (args["Activity"] is Activity activity && activity.Value != null && ((dynamic)activity.Value).chosenRoom is JValue chosenRoom)
                    {
                        var stateWrapper = new CheckRoomAvailabilityDialogStateWrapper(dc.ActiveDialog.State);

                        dynamic requestedBooking = JsonConvert.DeserializeObject<ExpandoObject>((string)chosenRoom.Value);
                        var meetingWebLink = await MicrosoftGraphExtensions.BookMicrosoftGraphMeeting(dc.Context.Services.Get<ConversationAuthToken>(AzureAdAuthMiddleware.AUTH_TOKEN_KEY).AccessToken,
                       "Booked meeting", requestedBooking.roomEmail, requestedBooking.start, requestedBooking.end);

                        var confirmation = activity.CreateReply();
                        CardExtensions.AddAdaptiveCardRoomConfirmationAttachment(confirmation, requestedBooking.roomEmail, requestedBooking.start.ToString("dd-MMM-yy HH:mm"), requestedBooking.end.ToString("dd-MMM-yy HH:mm"), meetingWebLink);
                        await dc.Context.SendActivity(confirmation);
                        await dc.End();
                    }
                    else
                    {
                        await dc.Begin(Id, dc.ActiveDialog.State);
                    }
                }
            });
        }
        public static string Id => "searchGraphDialog";

        public static SearchGraphDialog Instance = new SearchGraphDialog();
    }
}
