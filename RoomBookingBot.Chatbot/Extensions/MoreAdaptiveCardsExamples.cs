using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoomBookingBot.Chatbot.Extensions
{
    public static class MoreAdaptiveCardsExamples
    {
        public static void AddAdaptiveCardChoiceForm(this Activity activity, (string text, object value)[] choices)
        {
            activity.Attachments = new List<Attachment>() { CreateChoiceAdaptiveCardAttachment(choices) };

        }

        private static Attachment CreateChoiceAdaptiveCardAttachment((string text, object value)[] choices)
        {
            var choiceItems = new List<dynamic>(choices.Select(choice => new { title = choice.text, value = choice.value }));
            
            var serializedChoices = JsonConvert.SerializeObject(choiceItems.ToArray());

            var adaptiveCard = File.ReadAllText(@".\adaptivecard.choice.json");
            adaptiveCard = adaptiveCard.Replace("$(choices)", serializedChoices);

            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard)
            };
        }


    }
}
