using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using RoomBookingBot.Chatbot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability
{
    public class DisambiguateTimeDialog : DialogContainer
    {
        private DisambiguateTimeDialog() : base(Id)
        {
            var recognizer = new DateTimeRecognizer(Culture.English);
            var model = recognizer.GetDateTimeModel();

            Dialogs.Add(Id, new WaterfallStep[]
            {
                async(dc,args, next) =>{
                    await dc.Context.SendActivity("What time?");
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new DisambiguateTimeDialogStateWrapper(dc.ActiveDialog.State);

                    var value = model.Parse(dc.Context.Activity.Text).ParseRecognizer();

                    stateWrapper.Resolutions = value;

                    if (value.ResolutionType != Resolution.ResolutionTypes.Time && value.ResolutionType != Resolution.ResolutionTypes.TimeRange)
                    {
                        await dc.Context.SendActivity("I don't understand. Please provide a time");
                        await dc.Replace(Id);
                    }
                    else if (value.NeedsDisambiguation)
                    {
                        var amOrPmChoices = new List<Choice>(new []{new Choice() { Value = "AM" }, new Choice() { Value = "PM" } });
                        await dc.Prompt("choicePrompt", "Is that AM or PM?", new ChoicePromptOptions
                        {
                            Choices = amOrPmChoices
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        stateWrapper.Time = value.FirstOrDefault().Time1.Value;
                        await dc.End(dc.ActiveDialog.State);
                    }                    
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new DisambiguateTimeDialogStateWrapper(dc.ActiveDialog.State);
                    var amOrPmChoice = ((FoundChoice) args["Value"]).Value;
                    var availableTimes = stateWrapper.Resolutions.Select(x=>x.Time1.Value);
                    stateWrapper.Time = amOrPmChoice  == "AM" ? availableTimes.Min() : availableTimes.Max();
                    await dc.End(dc.ActiveDialog.State);
                }
            });

            Dialogs.Add("textPrompt", new TextPrompt());
            Dialogs.Add("choicePrompt", new ChoicePrompt("en"));
        }
        public static string Id => "disambiguateTimeDialog";

        public static DisambiguateTimeDialog Instance = new DisambiguateTimeDialog();
    }

    // convenience helper to get/set dialog state
    public class DisambiguateTimeDialogStateWrapper
    {
        public DisambiguateTimeDialogStateWrapper(IDictionary<string,object> state)
        {
            State = state;
        }

        public IDictionary<string, object> State { get; }

        public RecognizerExtensions.ResolutionList Resolutions {
            get
            {
                return State["recognizedDateTime"] as RecognizerExtensions.ResolutionList;
            }
            set
            {
                State["recognizedDateTime"] = value;
            }
        }

        public TimeSpan Time
        {
            get
            {
                return (TimeSpan)State["time"];
            }
            set
            {
                State["time"] = value;
            }
        }
    }
}
