using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using RoomBookingBot.Chatbot.Model;
using RoomBookingBot.Luis.Extensions;

namespace RoomBookingBot.Luis.Model
{
    public static partial class LuisModelParser
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
                    (result.Start, result.End) = entity.ProcessDateTimeV2DateTimeRange();
                }
                else if (entity.Type == "builtin.datetimeV2.datetime")
                {
                    result.Start = entity.ProcessDateTimeV2DateTime();
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
