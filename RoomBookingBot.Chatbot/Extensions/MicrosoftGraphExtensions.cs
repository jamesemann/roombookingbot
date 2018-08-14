using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace RoomBookingBot.Chatbot.Extensions
{
    public static class MicrosoftGraphExtensions
    {
        public static async Task<IUserPeopleCollectionPage> GetMicrosoftGraphFindMeetingRooms(string accessToken)
        {
            var graphClient = new GraphServiceClient(new PreAuthorizedBearerTokenAuthenticationProvider(accessToken));
            var rooms = await graphClient.Me.People.Request().Filter("personType/subclass eq 'Room'").GetAsync();
            return rooms;
        }

        public static async Task<string> BookMicrosoftGraphMeeting(string accessToken, string subject, string locationEmailAddress, DateTime start, DateTime end)
        {
            var graphClient = new GraphServiceClient(new PreAuthorizedBearerTokenAuthenticationProvider(accessToken));

            var createdEvent = await graphClient.Me.Events.Request().AddAsync(new Event
            {
                Subject = subject,
                Location = new Location {LocationEmailAddress = locationEmailAddress, LocationType = LocationType.ConferenceRoom, DisplayName = locationEmailAddress},
                Locations = new[] {new Location {LocationEmailAddress = locationEmailAddress, LocationType = LocationType.ConferenceRoom, DisplayName = locationEmailAddress}},
                Attendees = new[] {new Attendee {Type = AttendeeType.Resource, EmailAddress = new EmailAddress {Address = locationEmailAddress}}},
                Body = new ItemBody {Content = "booked by bot"},
                Start = new DateTimeTimeZone {TimeZone = "UTC", DateTime = start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss")},
                End = new DateTimeTimeZone {TimeZone = "UTC", DateTime = end.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss")}
            });
            return createdEvent.WebLink;
        }

        public static async Task<MeetingTimeSuggestionsResult> GetMicrosoftGraphFindMeeting(string accessToken, DateTime from, DateTime to, string meetingDuration, string[] meetingRoomEmailAddresses)
        {
            var locationConstraintItems = meetingRoomEmailAddresses.Select(meetingRoomEmailAddress => new LocationConstraintItem {LocationEmailAddress = meetingRoomEmailAddress});
            var attendeeItems = meetingRoomEmailAddresses.Select(meetingRoomEmailAddress => new Attendee {EmailAddress = new EmailAddress {Address = meetingRoomEmailAddress}});

            var graphClient = new GraphServiceClient(new PreAuthorizedBearerTokenAuthenticationProvider(accessToken));

            return await graphClient.Me.FindMeetingTimes(
                IsOrganizerOptional: true,
                MinimumAttendeePercentage: 0,
                Attendees: attendeeItems,
                LocationConstraint: new LocationConstraint
                {
                    IsRequired = false,
                    Locations = locationConstraintItems
                },
                TimeConstraint: new TimeConstraint
                {
                    Timeslots = new[]
                    {
                        new TimeSlot
                        {
                            Start = new DateTimeTimeZone {DateTime = from.ToString("yyyy-MM-ddTHH:mm:ss"), TimeZone = "UTC"},
                            End = new DateTimeTimeZone {DateTime = to.ToString("yyyy-MM-ddTHH:mm:ss"), TimeZone = "UTC"}
                        }
                    }
                },
                MeetingDuration: new Duration(meetingDuration)).Request().PostAsync();
        }

        private class PreAuthorizedBearerTokenAuthenticationProvider : IAuthenticationProvider
        {
            public PreAuthorizedBearerTokenAuthenticationProvider(string accessToken)
            {
                AccessToken = accessToken;
            }

            public string AccessToken { get; }

            public async Task AuthenticateRequestAsync(HttpRequestMessage request)
            {
                request.Headers.Add("Authorization", $"Bearer {AccessToken}");
            }
        }
    }
}