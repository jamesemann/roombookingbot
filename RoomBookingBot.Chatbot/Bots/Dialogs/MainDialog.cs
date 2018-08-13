using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using RoomBookingBot.Chatbot.Bots.Dialogs;
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
                        var dialogInput = args["Value"] as string;

                        if (string.IsNullOrEmpty(dialogInput))
                        {
                            dc.ActiveDialog.State["introduced"] = true;
                            await dc.Prompt("textPrompt", $"How can I help?");
                        }
                        else if (dialogInput == "Completed")
                        {
                            await dc.Prompt("textPrompt", $"Your meeting is booked, let me know if I can help with anything else");
                        }
                        else
                        {
                            dc.ActiveDialog.State["utterance"] = dialogInput;
                            await dc.Continue();
                        }
                    },
                    async (dc, args, next) =>
                    {
                        // the utterance could come from either 1) response to prompt in (args["Value"]) or 2) passed into dialog (dc.ActiveDialog.State["utterance"])
                        var utterance = args != null && args.ContainsKey("Value") ? (string)args["Value"] : (string)dc.ActiveDialog.State["utterance"];

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
                        // this is where we could detect other intents, like discover room, show room calendar, etc
                        else
                        {
                            await dc.Context.SendActivity($"Sorry, I don't know what you mean");
                        }
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["Value"] = "Completed";
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
