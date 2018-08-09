using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.DateTimeResult;

namespace RoomBookingBot.Chatbot.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDateTime(this List<DateTimeResolution> dateTimeResolutions)
        {
            return DateTime.Parse((dateTimeResolutions).FirstOrDefault().Value);
        }
    }
}
