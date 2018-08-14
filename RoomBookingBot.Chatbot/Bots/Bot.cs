using System.Collections.Generic;
using System.Threading.Tasks;
using JamesMann.BotFramework.Middleware.Extensions;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using RoomBookingBot.Chatbot.Bots.Dialogs;

namespace RoomBookingBot.Chatbot.Bots
{
    public class Bot : IBot
    {
        private readonly DialogSet dialogs;

        public Bot(IConfiguration configuration)
        {
            dialogs = new DialogSet();
            dialogs.Add("mainDialog", MainDialog.GetInstance(configuration));
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            if (turnContext.Activity.UserHasJustJoinedConversation() || turnContext.Activity.UserHasJustSentMessage())
            {
                var state = turnContext.GetConversationState<Dictionary<string, object>>();
                var dialogCtx = dialogs.CreateContext(turnContext, state);

                await dialogCtx.Continue();
                if (!turnContext.Responded)
                {
                    await dialogCtx.Begin("mainDialog", new Dictionary<string, object>
                    {
                        ["Value"] = turnContext.Activity.Text
                    });
                }
            }
        }
    }
}