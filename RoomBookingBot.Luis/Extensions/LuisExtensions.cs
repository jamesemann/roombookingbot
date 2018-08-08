using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoomBookingBot.Luis.Extensions
{
    public static class LuisExtensions
    {
        public static BookingRequest ParseLuisBookingRequest(this LuisResult luisResult)
        {
            var result = new BookingRequest();
            foreach (var entity in luisResult.Entities)
            {
                if (entity.Type == "builtin.datetimeV2.date")
                {
                    result.Start = ProcessDateTimeV2Date(entity);
                }
                if (entity.Type == "builtin.datetimeV2.datetimerange")
                {
                    (result.Start, result.End) = ProcessDateTimeV2DateTimeRange(entity);
                }
                else if (entity.Type == "builtin.datetimeV2.datetime")
                {
                    result.Start = ProcessDateTimeV2DateTime(entity);
                }
                else if (entity.Type == "Room")
                {
                    result.Room = ProcessRoom(entity);
                }
            }
            return result;
        }

        public class BookingRequest
        {
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }
            public string Room { get; set; }

            public override string ToString()
            {
                var result = $"Booking ";
                if (Start.HasValue)
                {
                    result += $"from: {Start.Value} ";
                }
                if (End.HasValue)
                {
                    result += $"to: {End.Value} ";
                }
                if (!string.IsNullOrEmpty(Room))
                {
                    result += $"room: {Room}";
                }
                return result;
            }
        }

        private static DateTime ProcessDateTimeV2Date(EntityModel entity)
        {
            if (entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
            {
                var resolutionValues = (IEnumerable<dynamic>)resolution.values;
                var datetimes = resolutionValues.Select(val => DateTime.Parse(val.value.Value));

                if (datetimes.Count() > 1)
                {
                    // assume the date is in the next 7 days
                    var bestGuess = datetimes.Single(xz => xz > DateTime.Now && (DateTime.Now - xz).Days <= 7);
                    return bestGuess;
                }

                return datetimes.FirstOrDefault();
            }

            throw new Exception("ProcessDateTimeV2DateTime");
        }

        private static string ProcessRoom(EntityModel entity)
        {
            if (entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
            {
                var resolutionValues = (IEnumerable<dynamic>)resolution.values;
                return resolutionValues.Select(val => val).FirstOrDefault();
            }

            throw new Exception("ProcessRoom");
        }

        private static DateTime ProcessDateTimeV2DateTime(EntityModel entity)
        {
            if (entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
            {
                var resolutionValues = (IEnumerable<dynamic>)resolution.values;
                var datetimes = resolutionValues.Select(val => DateTime.Parse(val.value.Value));

                if (datetimes.Count() > 1)
                {
                    // assume the date is in the next 7 days and falls between 7 AM - 7 PM is the most appropriate date 
                    var bestGuess = datetimes.Single(xz => xz.Hour > 7 && xz.Hour <= 19 && xz.start > DateTime.Now && (DateTime.Now - xz.start).Days <= 7);
                    return bestGuess;
                }

                return datetimes.FirstOrDefault();
            }

            throw new Exception("ProcessDateTimeV2DateTime");
        }

        private static (DateTime start, DateTime end) ProcessDateTimeV2DateTimeRange(EntityModel entity)
        {
            if (entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
            {
                var resolutionValues = (IEnumerable<dynamic>)resolution.values;
                var datetimeranges = resolutionValues.Select(val => new { start = DateTime.Parse(val.start.Value), end = DateTime.Parse(val.end.Value) });

                if (datetimeranges.Count() > 1)
                {
                    // assume the date is in the next 7 days and falls between 7 AM - 7 PM is the most appropriate date 
                    var bestGuess = datetimeranges.Single(xz => xz.start.Hour > 7 && xz.start.Hour <= 19 && xz.start > DateTime.Now && (DateTime.Now - xz.start).Days <= 7);
                    return (bestGuess.start, bestGuess.end);
                }

                return (datetimeranges.FirstOrDefault().start, datetimeranges.FirstOrDefault().end);
            }

            throw new Exception("ProcessDateTimeV2DateTimeRange");
        }
    }
}
