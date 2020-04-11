using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Xml.Linq;
using HealthCheck;
using HealthCheck.Framework;
using Moq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using Xunit;

namespace UnitTests
{
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Test Suites do not need XML Documentation.")]
    public class HealthServiceTests
    {
        [Fact]
        public void Constructor_Should_InstantiateHealthService()
        {
            // Arrange & Act
            var actual = new HealthService();

            // Assert
            Assert.IsType<HealthService>(actual);
            actual.Stop();
        }

        [Fact]
        public void NewJob_Should_ReturnNull_When_GroupDoesNotContainChecks()
        {
            // Arrange
            var service = new HealthService();

            service.Groups.Add(new HealthCheckGroup()
            {
                Name = "UnitTest"
            });

            var scheduler = new Mock<IScheduler>();
            var jobDetail = new JobDetailImpl()
            {
                Key = new JobKey("UnitTest", Guid.NewGuid().ToString()),
                Name = "NonExist"
            };

            var now = DateTime.Now;
            var calendar = new DailyCalendar(now.AddSeconds(0 - now.TimeOfDay.TotalSeconds), now.AddHours(1));
            var trigger = new CronTriggerImpl("NullTest", "g1", "* * * ? * * *");

            var bundle = new TriggerFiredBundle(
                jobDetail,
                trigger,
                calendar,
                false,
                new DateTimeOffset(now),
                new DateTimeOffset(now),
                new DateTimeOffset(now.AddSeconds(-1)),
                new DateTimeOffset(now.AddSeconds(1)));

            // Act
            var job = service.NewJob(bundle, scheduler.Object);

            // Assert
            Assert.Null(job);
        }

        [Fact]
        public void NewJob_Should_ReturnNull_When_JobDoesNotExist()
        {
            // Arrange
            var service = new HealthService();

            service.Groups.Add(new HealthCheckGroup()
            {
                Name = "UnitTest"
            });

            service.Groups[0].Checks.Add(new HealthCheckJob());

            var scheduler = new Mock<IScheduler>();
            var jobDetail = new JobDetailImpl()
            {
                Key = new JobKey("UnitTest", Guid.NewGuid().ToString()),
                Name = "NonExist"
            };

            var now = DateTime.Now;
            var calendar = new DailyCalendar(now.AddSeconds(0 - now.TimeOfDay.TotalSeconds), now.AddHours(1));
            var trigger = new CronTriggerImpl("NullTest", "g1", "* * * ? * * *");

            var bundle = new TriggerFiredBundle(
                jobDetail,
                trigger,
                calendar,
                false,
                new DateTimeOffset(now),
                new DateTimeOffset(now),
                new DateTimeOffset(now.AddSeconds(-1)),
                new DateTimeOffset(now.AddSeconds(1)));

            // Act
            var job = service.NewJob(bundle, scheduler.Object);

            // Assert
            Assert.Null(job);
        }

        [Fact]
        public void Start_Should_RepeatedlyExecuteJob()
        {
            // Arrange
            var counter = 0;

            var factory = new Mock<IComponentFactory>();
            var manager = new PluginManager(factory.Object);
            var reader = new Mock<IHealthCheckConfigurationReader>();
            var plugin = new Mock<IHealthCheckPlugin>();

            var group = new HealthCheckGroup()
            {
                Name = "UnitTest"
            };

            var job = new HealthCheckJob();
            job.Plugin = plugin.Object;
            job.JobConfiguration = new JobConfiguration();
            job.JobConfiguration.Triggers = new List<XElement>();
            job.JobConfiguration.Triggers.Add(new XElement("Trigger"));

            group.Checks.Add(job);

            var groups = new List<HealthCheckGroup>();
            groups.Add(group);

            reader.Setup(m => m.ReadAll()).Returns(groups);

            plugin.Setup(p => p.Execute())
                .Callback(() =>
                {
                    counter++;
                })
                .Returns(new HealthStatus());

            factory.Setup(f => f.GetPlugin(It.IsAny<string>())).Returns(plugin.Object);
            factory.Setup(m => m.GetTrigger(It.IsAny<XElement>()))
                .Returns(new CronTriggerImpl("StartTest", "g1", "* * * ? * * *"));

            var service = new HealthService(reader.Object, manager);

            // Act
            service.Start();
            Thread.Sleep(5000);

            // Assert
            Assert.True(counter > 1);
            service.Stop();
        }

        [Fact]
        public void Stop_Should_StopExecutingJobs()
        {
            // Arrange
            var counter = 0;

            var factory = new Mock<IComponentFactory>();
            var manager = new PluginManager(factory.Object);
            var reader = new Mock<IHealthCheckConfigurationReader>();
            var plugin = new Mock<IHealthCheckPlugin>();

            var group = new HealthCheckGroup()
            {
                Name = "UnitTest"
            };

            var job = new HealthCheckJob();
            job.Plugin = plugin.Object;
            job.JobConfiguration = new JobConfiguration();
            job.JobConfiguration.Triggers = new List<XElement>();
            job.JobConfiguration.Triggers.Add(new XElement("Trigger"));

            group.Checks.Add(job);

            var groups = new List<HealthCheckGroup>();
            groups.Add(group);

            reader.Setup(m => m.ReadAll()).Returns(groups);

            plugin.Setup(p => p.Execute())
                .Callback(() =>
                {
                    counter++;
                })
                .Returns(new HealthStatus());

            factory.Setup(f => f.GetPlugin(It.IsAny<string>())).Returns(plugin.Object);
            factory.Setup(m => m.GetTrigger(It.IsAny<XElement>()))
                .Returns(new CronTriggerImpl("StopTest", "g1", "* * * ? * * *"));

            var service = new HealthService(reader.Object, manager);

            // Act
            service.Start();
            Thread.Sleep(3000);
            var countAtStop = counter;
            service.Stop();
            Thread.Sleep(3000);

            // Assert
            Assert.Equal(countAtStop, counter);
        }
    }
}
