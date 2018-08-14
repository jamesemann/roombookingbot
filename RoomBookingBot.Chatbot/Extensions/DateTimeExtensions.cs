using System.Collections.Generic;
using System.Linq;
using static Microsoft.Bot.Builder.Prompts.DateTimeResult;

namespace RoomBookingBot.Chatbot.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToTimex(this List<DateTimeResolution> dateTimeResolutions)
        {
            return dateTimeResolutions.FirstOrDefault().Timex;
        }
    }
}