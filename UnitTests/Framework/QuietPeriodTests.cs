using System.Diagnostics.CodeAnalysis;
using Xunit;
using Quartz.Impl.Calendar;
using HealthCheck.Framework;
using System;

namespace UnitTests.Framework
{
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Test Suites do not need XML Documentation.")]
    public class QuietPeriodTests
    {
        [Fact]
        public void AddCalendar_Should_AddTheCalenderToList()
        {
            // Arrange
            var calendar = new AnnualCalendar();
            var quietPeriods = new QuietPeriods();

            // Act
            quietPeriods.AddCalendar(calendar);

            // Assert
            Assert.Equal(1, quietPeriods.Count);
        }

        [Fact]
        public void ClearCalendar_Should_RemoveAllCalendersFromList()
        {
            // Arrange
            var calendar1 = new AnnualCalendar();
            var calendar2 = new WeeklyCalendar();

            var quietPeriods = new QuietPeriods();
            quietPeriods.AddCalendar(calendar1);
            quietPeriods.AddCalendar(calendar2);

            // Act
            quietPeriods.ClearCalendars();

            // Assert
            Assert.Equal(0, quietPeriods.Count);
        }

        [Fact]
        public void Count_Should_BeZero_When_InitiallyCreated()
        {
            // Arrange & Act
            var quietPeriods = new QuietPeriods();

            // Assert
            Assert.Equal(0, quietPeriods.Count);
        }

        [Fact]
        public void IsQuietPeriod_Should_ReturnFalse_WhenEventTimeIsNotInQuietPeriod()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, 14, 7, 0, 0, DateTimeKind.Local));

            var calendar1 = new CronCalendar("* * 6 ? * * *"); // 6:00 AM until 6:59 AM, every day
            var calendar2 = new CronCalendar("* * * 15 * ? *"); // all day but only the 15th of the month

            var quietPeriods = new QuietPeriods();
            quietPeriods.AddCalendar(calendar1);
            quietPeriods.AddCalendar(calendar2);

            // Act
            var quiet = quietPeriods.IsQuietPeriod(eventTime);

            // Assert
            Assert.False(quiet);
        }

        [Fact]
        public void IsQuietPeriod_Should_ReturnTrue_WhenEventTimeIsInQuietPeriod()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, today.Day, 6, 30, 00, DateTimeKind.Local));

            var calendar1 = new CronCalendar("* * 6 ? * * *"); // 6:00 AM until 6:59 AM, every day
            var calendar2 = new CronCalendar("* * * 15 * ? *"); // all day but only the 15th of the month

            var quietPeriods = new QuietPeriods();
            quietPeriods.AddCalendar(calendar1);
            quietPeriods.AddCalendar(calendar2);

            // Act
            var quiet = quietPeriods.IsQuietPeriod(eventTime);

            // Assert
            Assert.True(quiet);
        }

        [Fact]
        public void IsQuietPeriodWithDailyCalendars_Should_ReturnFalse_WhenEventTimeIsNotInQuietPeriod()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, today.Day, 7, 30, 00, DateTimeKind.Local));

            var startTime = new DateTime(today.Year, today.Month, today.Day, 6, 0, 00, DateTimeKind.Local);
            var endTime = new DateTime(today.Year, today.Month, today.Day, 6, 59, 00, DateTimeKind.Local);
            var calendar1 = new DailyCalendar(startTime, endTime); // 6:00 AM until 6:59 AM, every day

            startTime = new DateTime(today.Year, today.Month, today.Day, 14, 0, 00, DateTimeKind.Local);
            endTime = new DateTime(today.Year, today.Month, today.Day, 14, 59, 00, DateTimeKind.Local);
            var calendar2 = new DailyCalendar(startTime, endTime); // 6:00 AM until 6:59 AM, every day

            var quietPeriods = new QuietPeriods();
            quietPeriods.AddCalendar(calendar1);
            quietPeriods.AddCalendar(calendar2);

            // Act
            var quiet = quietPeriods.IsQuietPeriod(eventTime);

            // Assert
            Assert.False(quiet);
        }

        [Fact]
        public void IsQuietPeriodWithDailyCalendars_Should_ReturnTrue_WhenEventTimeIsInQuietPeriod()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, today.Day, 14, 30, 00, DateTimeKind.Local).AddDays(2));

            var startTime = new DateTime(today.Year, today.Month, today.Day, 6, 0, 00, DateTimeKind.Local);
            var endTime = new DateTime(today.Year, today.Month, today.Day, 6, 59, 00, DateTimeKind.Local);
            var calendar1 = new DailyCalendar(startTime, endTime); // 6:00 AM until 6:59 AM, every day

            startTime = new DateTime(today.Year, today.Month, today.Day, 14, 0, 00, DateTimeKind.Local);
            endTime = new DateTime(today.Year, today.Month, today.Day, 14, 59, 00, DateTimeKind.Local);
            var calendar2 = new DailyCalendar(startTime, endTime); // 6:00 AM until 6:59 AM, every day

            var quietPeriods = new QuietPeriods();
            quietPeriods.AddCalendar(calendar1);
            quietPeriods.AddCalendar(calendar2);

            // Act
            var quiet = quietPeriods.IsQuietPeriod(eventTime);

            // Assert
            Assert.True(quiet);
        }

        [Fact]
        public void QuartzCronCalendar_Should_ReturnFalse_When_EventTimeInQuietPeriod()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, today.Day, 6, 30, 00, DateTimeKind.Local));
            var calendar = new CronCalendar("* * 6 ? * * *"); // 6:00 AM until 6:59 AM, every day

            // Act
            var quiet = calendar.IsTimeIncluded(eventTime);

            // Assert
            Assert.False(quiet);
        }

        [Fact]
        public void QuartzCronCalendar_Should_ReturnTrue_When_EventTimeIsNotInQuietPeriod()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, today.Day, 7, 30, 00, DateTimeKind.Local));
            var calendar = new CronCalendar("* * 6 ? * * *"); // 6:00 AM until 6:59 AM, every day

            // Act
            var quiet = calendar.IsTimeIncluded(eventTime);

            // Assert
            Assert.True(quiet);
        }

        [Fact]
        public void QuartzDailyCalendar_Should_ReturnTrue_When_EventTimeIsNotInQuietPeriod()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, today.Day, 7, 30, 00, DateTimeKind.Local));
            var startTime = new DateTime(today.Year, today.Month, today.Day, 6, 0, 00, DateTimeKind.Local);
            var endTime = new DateTime(today.Year, today.Month, today.Day, 6, 59, 00, DateTimeKind.Local);
            var calendar = new DailyCalendar(startTime, endTime); // 6:00 AM until 6:59 AM, every day

            // Act
            var quiet = calendar.IsTimeIncluded(eventTime);

            // Assert
            Assert.True(quiet);
        }

        [Fact]
        public void QuartzDaliyCalendar_Should_ReturnFalse_When_EventTimeInQuietPeriod()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, today.Day, 6, 30, 00, DateTimeKind.Local));
            var startTime = new DateTime(today.Year, today.Month, today.Day, 6, 0, 00, DateTimeKind.Local);
            var endTime = new DateTime(today.Year, today.Month, today.Day, 6, 59, 00, DateTimeKind.Local);
            var calendar = new DailyCalendar(startTime, endTime); // 6:00 AM until 6:59 AM, every day

            // Act
            var quiet = calendar.IsTimeIncluded(eventTime);

            // Assert
            Assert.False(quiet);
        }

        [Fact]
        public void RemoveCalendar_Should_RemoveTheCalenderFromList()
        {
            // Arrange
            var calendar1 = new AnnualCalendar();
            var calendar2 = new WeeklyCalendar();

            var quietPeriods = new QuietPeriods();
            quietPeriods.AddCalendar(calendar1);
            quietPeriods.AddCalendar(calendar2);

            // Act
            quietPeriods.RemoveCalendar(calendar1);

            // Assert
            Assert.Equal(1, quietPeriods.Count);
        }
    }
}
