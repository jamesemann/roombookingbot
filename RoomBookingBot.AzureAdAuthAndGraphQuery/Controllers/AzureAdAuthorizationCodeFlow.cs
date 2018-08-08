using Microsoft.Extensions.Configuration;
using RoomBookingBot.Extensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoomBookingBot.Controllers
{
    public class AzureAdAuthorizationCodeFlow
    {
        readonly string azureAdTenant;
        readonly string clientId;
        readonly string userConsentRedirectUri;
        readonly string permissionsRequested;
        readonly string clientSecret;

        public AzureAdAuthorizationCodeFlow(IConfiguration configuration)
        {
            azureAdTenant = configuration.GetValue<string>("azureAdTenant");
            clientId = configuration.GetValue<string>("clientId");
            userConsentRedirectUri = configuration.GetValue<string>("userConsentRedirectUri");
            permissionsRequested = configuration.GetValue<string>("permissionsRequested");
            clientSecret = configuration.GetValue<string>("clientSecret");

            Console.WriteLine($"Provide user consent at: https://login.microsoftonline.com/{azureAdTenant}/oauth2/v2.0/authorize?client_id={clientId}&scope={permissionsRequested}&response_type=code&response_mode=query&redirect_uri={userConsentRedirectUri}&state=12345");
        }

        public async Task UserConsented(string code, string state)
        {
            var httpClient = new HttpClient();

            // get an access token from azure ad
            var accessToken = await httpClient.GetAzureAdToken(azureAdTenant, code, clientId, userConsentRedirectUri, clientSecret, permissionsRequested);

            // find 1 hour meeting slots in the next 24 hours in room 
            var meetingTimes = await MicrosoftGraphExtensions.GetMicrosoftGraphFindMeeting(accessToken, DateTime.Now, DateTime.Now.AddDays(24), "PT1H", "Board room", "boardroom@jemann.onmicrosoft.com");
            foreach (var meeting in meetingTimes.MeetingTimeSuggestions)
            {
                Console.WriteLine($"Meeting from {DateTime.Parse(meeting.MeetingTimeSlot.Start.DateTime)} to {DateTime.Parse(meeting.MeetingTimeSlot.End.DateTime)}");
            }
        }
    }
}
