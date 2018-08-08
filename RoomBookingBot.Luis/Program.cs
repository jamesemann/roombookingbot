using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using RoomBookingBot.Luis.Extensions;
using System;
using System.Threading.Tasks;

namespace RoomBookingBot.Luis
{
    class Program
    {
        private readonly static string apiKey = "<ADD YOUR LUIS KEY>";
        private readonly static string modelId = "<ADD YOUR LUIS MODEL ID>";


        public async static Task Main(string[] args)
        {
            var cli = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                BaseUri = new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0")
            };

            while (true)
            {
                var utterance = Console.ReadLine();
                var prediction = await cli.Prediction.ResolveWithHttpMessagesAsync(modelId, utterance);
                if (prediction.Body.TopScoringIntent.Intent == "check-room-availability")
                {
                    var bookingRequest = prediction.Body.ParseLuisBookingRequest();
                    Console.WriteLine($"check-room-availability: {bookingRequest}");
                }
                else if (prediction.Body.TopScoringIntent.Intent == "discover-rooms")
                {
                    Console.WriteLine($"discover-rooms");
                }
                else
                {
                    Console.WriteLine($"unknown");
                }
            }
        }
    }
}
