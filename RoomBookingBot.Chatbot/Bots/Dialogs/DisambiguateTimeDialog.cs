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
                    if (args!= null && args.ContainsKey("timeProvided") && (Boolean)(args["timeProvided"]) && ((TimeSpan)args["time"]).Hours < 7)
                    {
                        await dc.Context.SendActivity("Sorry, the meeting rooms are not open at that time");
                        await dc.Continue();
                    }
                    else if (args!= null && args.ContainsKey("timeProvided") && (Boolean)(args["timeProvided"]))
                    {
                        await dc.End(args);
                    }
                    else
                    {
                        await dc.Continue();
                    }
                },
                async(dc,args, next) =>{
                    await dc.Context.SendActivity("The rooms are available from 7AM");
                    await dc.Prompt("dateTimePrompt","What time would you like your meeting to start?");
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
