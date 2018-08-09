using Microsoft.Bot.Builder;
using RoomBookingBot.Chatbot.Extensions;
using System.Threading.Tasks;

namespace RoomBookingBot.Chatbot.Middleware
{
    public class TypingMiddleware : IMiddleware
    {
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Activity.UserHasJustJoinedConversation() || context.Activity.UserHasJustSentMessage())
            {
                await context.Activity.DoWithTyping(async () =>
                {
                    await next();
                });
            }
            else
            {
                await next();
            }
        }
    }
}
