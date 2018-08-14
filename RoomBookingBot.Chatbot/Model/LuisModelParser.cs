using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using RoomBookingBot.Chatbot.Extensions;

namespace RoomBookingBot.Chatbot.Model
{
    public static class LuisModelParser
    {
        public static BookingRequest ParseLuisBookingRequest(this LuisResult luisResult)
        {
            var result = new BookingRequest();
            foreach (var entity in luisResult.Entities)
            {
                if (entity.Type == "builtin.datetimeV2.date")
                {
                    result.Start = entity.ProcessDateTimeV2Date();
                }

                if (entity.Type == "builtin.datetimeV2.datetimerange")
                {
                    (result.Start, result.MeetingDuration) = entity.ProcessDateTimeV2DateTimeRange();
                }
                else if (entity.Type == "builtin.datetimeV2.datetime")
                {
                    result.Start = entity.ProcessDateTimeV2DateTime();
                }
                else if (entity.Type == "builtin.datetimeV2.duration")
                {
                    result.MeetingDuration = entity.ProcessDateTimeV2Duration();
                }
                else if (entity.Type == "Room")
                {
                    result.Room = entity.ProcessRoom();
                }
            }

            return result;
        }
    }
}