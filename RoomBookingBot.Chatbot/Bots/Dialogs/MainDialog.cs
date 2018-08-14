using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using RoomBookingBot.Chatbot.Bots.Dialogs.Intents.CheckRoomAvailability;
using RoomBookingBot.Chatbot.Model;

namespace RoomBookingBot.Chatbot.Bots.Dialogs
{
    public class MainDialog : DialogContainer
    {
        private MainDialog(IConfiguration configuration) : base(Id)
        {
            LuisModelId = configuration.GetValue<string>("LuisModelId");
            LuisModelKey = configuration.GetValue<string>("LuisModelKey");
            LuisEndpoint = configuration.GetValue<string>("LuisEndpoint");

            Dialogs.Add(DialogId, new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        var dialogInput = args["Value"] as string;

                        if (string.IsNullOrEmpty(dialogInput))
                        {
                            // 8. introduce bot if its the first time a user has visited
                            await dc.Context.SendActivity($"How can I help?");
                            await dc.End();
                        }
                        else
                        {
                            // 9. otherwise, run NLP in LUIS to start the "Identify" stage
                            var cli = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(LuisModelKey)) {BaseUri = new Uri(LuisEndpoint)};
                            var prediction = await cli.Prediction.ResolveWithHttpMessagesAsync(LuisModelId, dialogInput);

                            if (prediction.Body.TopScoringIntent.Intent == "check-room-availability")
                            {
                                var bookingRequest = prediction.Body.ParseLuisBookingRequest();
                                var checkRoomAvailabilityDialogArgs = new Dictionary<string, object> {{"bookingRequest", bookingRequest}};
                                // 10. if the top scoring intent is check-room-availability, then transition into the CheckRoomAvailability dialog
                                await dc.Begin(CheckRoomAvailabilityDialog.Id, checkRoomAvailabilityDialogArgs);
                            }
                            else
                            {
                                await dc.Context.SendActivity($"Sorry, I don't know what you mean");
                                await dc.End();
                            }
                        }
                    },
                    async (dc, args, next) =>
                    {
                        await dc.Prompt("textPrompt", $"Please let me know if I can help with anything else");
                        await dc.End();
                    }
                }
            );

            Dialogs.Add(CheckRoomAvailabilityDialog.Id, CheckRoomAvailabilityDialog.Instance);
            Dialogs.Add("textPrompt", new TextPrompt());
        }

        public static string Id => "mainDialog";

        private static MainDialog instance { get; set; }

        public string LuisModelId { get; }
        public string LuisModelKey { get; }
        public string LuisEndpoint { get; }

        public static MainDialog GetInstance(IConfiguration configuration)
        {
            return instance ?? (instance = new MainDialog(configuration));
        }
    }
}