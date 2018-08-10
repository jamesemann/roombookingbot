using Microsoft.Bot.Builder.Dialogs;
using RoomBookingBot.Chatbot.Extensions;
using System;
using System.Collections.Generic;
using static Microsoft.Bot.Builder.Prompts.DateTimeResult;

namespace RoomBookingBot.Chatbot.Dialogs.CheckRoomAvailability
{
    public class DisambiguateTimeDialog : DialogContainer
    {
        private DisambiguateTimeDialog() : base(Id)
        {
            Dialogs.Add(Id, new WaterfallStep[]
            {
                async (dc, args, next) => {
                    if (args!= null && args.ContainsKey("timeProvided") && (Boolean)(args["timeProvided"]))
                    {
                        await dc.End(args);
                    }
                    else
                    {
                        await dc.Prompt("dateTimePrompt", (args != null && args.ContainsKey("message") ? (string)args["message"] : "What time?"));
                    }
                },
                async (dc, args, next) =>
                {
                    (var start, var startContainsTimePart) = (args["Resolution"] as List<DateTimeResolution>).ToDateTime();
                    dc.ActiveDialog.State["timeProvided"] = startContainsTimePart;
                    dc.ActiveDialog.State["time"] = start.TimeOfDay;
                    await dc.Replace(Id,dc.ActiveDialog.State);
                },
            });

            Dialogs.Add("dateTimePrompt", new DateTimePrompt("en"));
        }
        public static string Id => "disambiguateTimeDialog";

        public static DisambiguateTimeDialog Instance = new DisambiguateTimeDialog();

        
    }
}
