using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability;
using RoomBookingBot.Chatbot.Model;
using System;
using System.Collections.Generic;

namespace RoomBookingBot.Chatbot.Dialogs
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
                        var botHasAlreadyIntroducedSelf = dc.ActiveDialog.State.ContainsKey("introduced") && (bool)dc.ActiveDialog.State["introduced"];

                        if (!botHasAlreadyIntroducedSelf)
                        {
                            dc.ActiveDialog.State["introduced"] = true;
                            await dc.Prompt("textPrompt", $"How can I help?");
                        }
                        else
                        {
                            await dc.Continue();
                        }
                    },
                    async (dc, args, next) =>
                    {
                        var utterance = args["Value"] as string;

                        var cli = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(LuisModelKey))
                        {
                            BaseUri = new Uri(LuisEndpoint)
                        };
                        var prediction = await cli.Prediction.ResolveWithHttpMessagesAsync(LuisModelId, utterance);

                        if (prediction.Body.TopScoringIntent.Intent == "check-room-availability")
                        {
                            var bookingRequest = prediction.Body.ParseLuisBookingRequest();
                            var checkRoomAvailabilityDialogArgs = new Dictionary<string,object>{{"bookingRequest", bookingRequest}};
                            await dc.Begin(CheckRoomAvailabilityDialog.Id, checkRoomAvailabilityDialogArgs);
                        }
                        else if (prediction.Body.TopScoringIntent.Intent == "discover-rooms")
                        {
                            await dc.Context.SendActivity($"discover-rooms");
                        }
                        else
                        {
                            await dc.Context.SendActivity($"unknown");
                        }
                    },
                    async (dc, args, next) =>
                    {
                        await dc.Replace(Id, dc.ActiveDialog.State);
                    }
                }
            );

            Dialogs.Add(CheckRoomAvailabilityDialog.Id, CheckRoomAvailabilityDialog.Instance);
            Dialogs.Add("textPrompt", new TextPrompt());
        }

        public static string Id => "mainDialog";

        private static MainDialog instance { get; set; }

        public static MainDialog GetInstance(IConfiguration configuration)
        {
            if (instance == null)
            {
                instance = new MainDialog(configuration);
            }
            return instance;
        }
        
        public string LuisModelId { get; }
        public string LuisModelKey { get; }
        public string LuisEndpoint { get; }
    }
}
