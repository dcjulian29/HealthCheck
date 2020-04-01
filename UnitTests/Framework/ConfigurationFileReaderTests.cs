using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using HealthCheck.Framework;
using Quartz.Impl.Calendar;
using Xunit;

namespace UnitTests.Framework
{
    [SuppressMessage(
         "StyleCop.CSharp.DocumentationRules",
         "SA1600:ElementsMustBeDocumented",
         Justification = "Test Suites do not need XML Documentation.")]
    public class ConfigurationFileReaderTests
    {
        public ConfigurationFileReaderTests()
        {
            // Various tests will copy configuration files into the directory so always start with a
            // clean slate.
            while (Directory.Exists("conf"))
            {
                try // Keep trying until directory empty
                {
                    Directory.Delete("conf", true);
                }
                catch (IOException)
                {
                    ((Action)(() => { }))(); // noop
                }
            }

            Directory.CreateDirectory("conf");
        }

        [Fact]
        public void Read_Should_ReturnCronCalendar_When_ProvidedCronElement()
        {
            // Arrange
            File.Copy("CalendarCronExclusion.xml", "conf\\calendar.xml");
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.Read("conf\\calendar.xml");
            var check = groups.FirstOrDefault().Checks.FirstOrDefault();

            // Assert
            _ = Assert.IsAssignableFrom<CronCalendar>(check.QuietPeriods.Calendars.FirstOrDefault());
        }

        [Fact]
        public void Read_Should_ReturnCronCalendars_When_ProvidedTwoCronElement()
        {
            // Arrange
            File.Copy("CalendarTwoCronExclusion.xml", "conf\\calendar.xml");
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.Read("conf\\calendar.xml");
            var check = groups.FirstOrDefault().Checks.FirstOrDefault();

            // Assert
            _ = Assert.IsAssignableFrom<CronCalendar>(check.QuietPeriods.Calendars[0]);
            _ = Assert.IsAssignableFrom<CronCalendar>(check.QuietPeriods.Calendars[1]);
        }

        [Fact]
        public void Read_Should_ReturnGroups_When_ProvidedProperXml()
        {
            // Arrange
            File.Copy("ValidConfig1.xml", "conf\\valid.xml");
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.Read("conf\\valid.xml");

            // Assert
            Assert.NotEmpty(groups);
        }

        [Fact]
        public void Read_Should_ReturnNoCalendars_When_ProvidedUnknownElement()
        {
            // Arrange
            File.Copy("CalendarInvalidExclusion.xml", "conf\\calendar.xml");
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.Read("conf\\calendar.xml");
            var check = groups.FirstOrDefault().Checks.FirstOrDefault();

            // Assert
            Assert.Equal(0, check.QuietPeriods.Count);
        }

        [Fact]
        public void Read_Should_ReturnNoGroups_When_ProvidedXmlWithMissingElement()
        {
            // Arrange
            File.Copy("InValidConfig.xml", "conf\\invalid.xml");
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.Read("conf\\invalid.xml");

            // Assert
            Assert.Empty(groups);
        }

        [Fact]
        public void Read_Should_ReturnNoGroups_When_ProvidedXmlWithoutHealthChecks()
        {
            // Arrange
            File.Copy("EmptyHealthChecks.xml", "conf\\invalid.xml");
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.Read("conf\\invalid.xml");

            // Assert
            Assert.Empty(groups);
        }

        [Fact]
        public void Read_Should_ReturnParsedExclusionCorrectly()
        {
            // Arrange
            File.Copy("CalendarCronExclusion.xml", "conf\\calendar.xml");
            var reader = new ConfigurationFileReader();
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, today.Day, 3, 14, 15, DateTimeKind.Local));

            // Act
            var groups = reader.Read("conf\\calendar.xml");
            var check = groups.FirstOrDefault().Checks.FirstOrDefault();

            // Assert
            Assert.True(check.QuietPeriods.IsQuietPeriod(eventTime));
        }

        [Fact]
        public void Read_Should_ReturnParsedExclusionCorrectly_When_ProvidedMultipleQuietPeriods()
        {
            // Arrange
            File.Copy("CalendarTwoCronExclusion.xml", "conf\\calendar.xml");
            var reader = new ConfigurationFileReader();
            var today = DateTime.UtcNow;
            var eventTime = new DateTimeOffset(
                new DateTime(today.Year, today.Month, 15, 7, 0, 0, DateTimeKind.Local));

            // Act
            var groups = reader.Read("conf\\calendar.xml");
            var check = groups.FirstOrDefault().Checks.FirstOrDefault();

            // Assert
            Assert.True(check.QuietPeriods.IsQuietPeriod(eventTime));
        }

        [Fact]
        public void ReadAll_Should_ReturnEmptyGroupList_When_DirectoryDoesNotExist()
        {
            // Arrange
            Directory.Delete("conf", true);
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.ReadAll();

            // Assert
            Assert.Empty(groups);
        }

        [Fact]
        public void ReadAll_Should_ReturnUnionOfGroups_When_ProvidedProperXmlFiles()
        {
            // Arrange
            File.Copy("ValidConfig1.xml", "conf\\valid1.xml");
            File.Copy("ValidConfig2.xml", "conf\\valid2.xml");
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.ReadAll();

            // Assert
            Assert.Equal(3, groups.Count);
        }

        [Fact]
        public void ReadAll_Should_ThrowException_When_ProvidedDuplicateChecks()
        {
            // Arrange
            File.Copy("DuplicateCheck.xml", "conf\\DuplicateCheck.xml");
            var reader = new ConfigurationFileReader();

            // Act & Assert
            Assert.Throws<DuplicateHealthCheckException>(() =>
            {
                _ = reader.ReadAll();
            });
        }

        [Fact]
        public void ReadAll_Should_ThrowException_When_ProvidedDuplicateGroups()
        {
            // Arrange
            File.Copy("ValidConfig1.xml", "conf\\valid1.xml");
            File.Copy("ValidConfig1.xml", "conf\\valid2.xml");
            var reader = new ConfigurationFileReader();

            // Act & Assert
            Assert.Throws<DuplicateHealthCheckException>(() =>
            {
                _ = reader.ReadAll();
            });
        }
    }
}
