using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using RoomBookingBot.Chatbot.Extensions;
using System.Threading.Tasks;

namespace RoomBookingBot.Chatbot.Middleware
{
    public class SpellCheckMiddleware :IMiddleware
    {
        public SpellCheckMiddleware(IConfiguration configuration)
        {
            this.ApiKey = configuration.GetValue<string>("SpellCheckKey");
        }

        public string ApiKey { get; }

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            context.Activity.Text = await context.Activity.Text.SpellCheck(ApiKey);

            await next();
        }
    }
}
