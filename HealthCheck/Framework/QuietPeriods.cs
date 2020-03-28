using System;
using System.Collections.Generic;
using Common.Logging;
using Quartz;

namespace HealthCheck.Framework
{
    /// <summary>
    ///   Holds a list of quiet periods for health checks.
    /// </summary>
    public class QuietPeriods
    {
        private static ILog _log = LogManager.GetLogger<QuietPeriods>();

        /// <summary>
        ///   Initializes a new instance of the <see cref="QuietPeriods" /> class.
        /// </summary>
        public QuietPeriods()
        {
        }

        /// <summary>
        ///   Gets the list of calendars containing the quiet periods.
        /// </summary>
        public List<ICalendar> Calendars { get; } = new List<ICalendar>();

        /// <summary>
        ///   Gets the count of calendars
        /// </summary>
        public int Count => Calendars.Count;

        /// <summary>
        ///   Add a calendar to the list of quiet periods.
        /// </summary>
        /// <param name="calendar">The calendar to add.</param>
        public void AddCalendar(ICalendar calendar)
        {
            Calendars.Add(calendar);
        }

        /// <summary>
        ///   Clears the calendars.
        /// </summary>
        public void ClearCalendars()
        {
            Calendars.Clear();
        }

        /// <summary>
        ///   Determines whether the specified date is in the quiet period.
        /// </summary>
        /// <param name="date">The date and time to check</param>
        /// <returns><c>true</c> if the specified date is in the quiet period; otherwise, <c>false</c>.</returns>
        public bool IsQuietPeriod(DateTimeOffset date)
        {
            var quiet = false;

            foreach (var calendar in Calendars)
            {
                if (!calendar.IsTimeIncluded(date))
                {
                    _log.Debug(m => m($"'{date.ToString()}' is included in {calendar}"));
                    quiet = true;
                    break;
                }
            }

            return quiet;
        }

        /// <summary>
        ///   Remove a calendar from the list of quiet periods.
        /// </summary>
        /// <param name="calendar">The calendar to remove.</param>
        public void RemoveCalendar(ICalendar calendar)
        {
            Calendars.Remove(calendar);
        }
    }
}
