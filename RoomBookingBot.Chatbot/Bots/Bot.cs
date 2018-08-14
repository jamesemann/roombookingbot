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

            // 5. in our Bot, add MainDialog to the top of the stack
            dialogs.Add("mainDialog", MainDialog.GetInstance(configuration));
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            if (turnContext.Activity.UserHasJustJoinedConversation() || turnContext.Activity.UserHasJustSentMessage())
            {
                var state = turnContext.GetConversationState<Dictionary<string, object>>();
                var dialogCtx = dialogs.CreateContext(turnContext, state);

                // 7. subsequent turns will allow the dialog waterfall to execute
                await dialogCtx.Continue();
                if (!turnContext.Responded)
                {
                    // 6. on the first turn, begin the MainDialog
                    await dialogCtx.Begin("mainDialog", new Dictionary<string, object>
                    {
                        ["Value"] = turnContext.Activity.Text
                    });
                }
            }
        }
    }
}