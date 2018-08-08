using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoomBookingBot.Extensions
{
    public static class MicrosoftGraphExtensions
    {
        public static async Task<MeetingTimeSuggestionsResult> GetMicrosoftGraphFindMeeting(string accessToken, DateTime from, DateTime to, string meetingDuration, string meetingRoomDisplayName, string meetingRoomEmailAddress)
        {
            var graphClient = new GraphServiceClient(new PreAuthorizedBearerTokenAuthenticationProvider(accessToken));
            return await graphClient.Me.FindMeetingTimes(
                LocationConstraint: new LocationConstraint()
                {
                    IsRequired = false,
                    Locations = new LocationConstraintItem[] 
                    {
                        new LocationConstraintItem() { LocationEmailAddress = meetingRoomEmailAddress, DisplayName = meetingRoomDisplayName }
                    }
                }, 
                TimeConstraint: new TimeConstraint()
                {
                    Timeslots = new TimeSlot[] {
                        new TimeSlot()
                        {
                            Start = new DateTimeTimeZone() { DateTime = from.ToString("yyyy-MM-ddTHH:mm:ss"), TimeZone = "UTC" },
                            End = new DateTimeTimeZone() { DateTime = to.ToString("yyyy-MM-ddTHH:mm:ss"), TimeZone = "UTC" }
                        }
                    }
                },
                MeetingDuration: new Duration(meetingDuration)).Request().PostAsync();
        }

        class PreAuthorizedBearerTokenAuthenticationProvider : IAuthenticationProvider
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
