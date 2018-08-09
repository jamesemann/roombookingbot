using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability;
using RoomBookingBot.Luis.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomBookingBot.Chatbot.Dialogs
{
    public class MainDialog : DialogContainer
    {
        private MainDialog() : base(Id)
        {
            Dialogs.Add(DialogId, new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        await dc.Prompt("textPrompt", $"How can I help?");
                    },

                    async (dc, args, next) =>
                    {
                        var utterance = args["Value"] as string;

                        // ask LUIS
                        string apiKey = "";
                        string modelId = "";
                        var cli = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(apiKey))
                        {
                            BaseUri = new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0")
                        };
                        var prediction = await cli.Prediction.ResolveWithHttpMessagesAsync(modelId, utterance);
                        if (prediction.Body.TopScoringIntent.Intent == "check-room-availability")
                        {
                            var bookingRequest = prediction.Body.ParseLuisBookingRequest();
                            await dc.Context.SendActivity($"check-room-availability: {bookingRequest}");
                            var checkRoomAvailabilityDialogArgs = new Dictionary<string,object>();
                            checkRoomAvailabilityDialogArgs.Add("bookingRequest", bookingRequest);
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
                        await dc.Replace(Id);
                    }
                }
            );

            // add the child dialogs and prompts
            //Dialogs.Add(MakePaymentDialog.Id, MakePaymentDialog.Instance);
            //Dialogs.Add(CheckBalanceDialog.Id, CheckBalanceDialog.Instance);
            Dialogs.Add(CheckRoomAvailabilityDialog.Id, CheckRoomAvailabilityDialog.Instance);
            
            Dialogs.Add("textPrompt", new TextPrompt());
            //Dialogs.Add("choicePrompt", new ChoicePrompt("en"));
        }

        public static string Id => "mainDialog";

        public static MainDialog Instance { get; } = new MainDialog();
    }
}
