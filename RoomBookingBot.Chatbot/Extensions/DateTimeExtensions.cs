using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.DateTimeResult;

namespace RoomBookingBot.Chatbot.Extensions
{
    public static class DateTimeExtensions
    {
        public static (DateTime parsedDate, bool containsTimePart) ToDateTime(this List<DateTimeResolution> dateTimeResolutions)
        {
            var dateOnlyRegex = new Regex("^[0-9]{4}-[0-9]{2}-[0-9]{2}$");
            var value = (dateTimeResolutions).FirstOrDefault().Value;
            DateTime.TryParse(value, out DateTime result);
            return (result, !dateOnlyRegex.IsMatch(value));
        }

        public static TimeSpan ToTimeSpan(this List<DateTimeResolution> dateTimeResolutions)
        {
            var result = TimeSpan.FromSeconds(double.Parse((dateTimeResolutions).FirstOrDefault().Value));
            return result;
        }
    }
}
