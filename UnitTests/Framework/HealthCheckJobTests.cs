using System;
using System.Diagnostics.CodeAnalysis;
using HealthCheck;
using HealthCheck.Framework;
using Moq;
using Quartz.Impl.Calendar;
using Xunit;

namespace UnitTests.Framework
{
    [SuppressMessage(
         "StyleCop.CSharp.DocumentationRules",
         "SA1600:ElementsMustBeDocumented",
         Justification = "Test Suites do not need XML Documentation.")]
    public class HealthCheckJobTests
    {
        [Fact]
        public void Execute_Should_CallListernerAfterExecution()
        {
            // Arrange
            var executed = false;
            var mockPlugin = new Mock<IHealthCheckPlugin>();

            var mockStatus = new Mock<IHealthStatus>();
            mockStatus.SetupGet(s => s.Status).Returns(CheckResult.Success);
            mockPlugin.Setup(p => p.Execute()).Returns(mockStatus.Object);

            var job = new HealthCheckJob
            {
                Plugin = mockPlugin.Object
            };

            var mockListener = new Mock<IStatusListener>();
            mockListener.Setup(l => l.Process(It.IsAny<IHealthStatus>())).Callback(() =>
            {
                executed = true;
            });

            job.Listeners.Add(mockListener.Object);

            // Act
            job.Execute(null);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Execute_Should_ExecuteJob()
        {
            // Arrange
            var executed = false;
            var mock = new Mock<IHealthCheckPlugin>();
            mock.Setup(p => p.Execute()).Callback(() =>
            {
                executed = true;
            });

            var job = new HealthCheckJob
            {
                Plugin = mock.Object
            };

            // Act
            job.Execute(null);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Execute_Should_HandleExceptions()
        {
            // Arrange
            var mockPlugin = new Mock<IHealthCheckPlugin>();
            mockPlugin.Setup(p => p.Execute()).Throws(new Exception("BOOM!"));
            mockPlugin.SetupProperty(p => p.PluginStatus);

            var job = new HealthCheckJob
            {
                Plugin = mockPlugin.Object
            };

            // Act
            job.Execute(null);

            // Assert
            Assert.Equal(PluginStatus.TaskExecutionFailure, job.Plugin.PluginStatus);
        }

        [Fact]
        public void Execute_Should_NotExecuteJob_When_InQuietPeriod()
        {
            // Arrange
            var calendar = new DailyCalendar(0, 0, 0, 0, 23, 59, 59, 999);

            var executed = false;
            var mock = new Mock<IHealthCheckPlugin>();
            _ = mock.Setup(p => p.Execute()).Callback(() => executed = true);

            var job = new HealthCheckJob
            {
                Plugin = mock.Object
            };

            // Act
            job.QuietPeriods.AddCalendar(calendar);
            job.Execute(null);

            // Assert
            Assert.False(executed);
        }

        [Fact]
        public void NotifyListener_Should_CallListener_When_ThreshholdIsOverThreshold()
        {
            // Arrange
            var executed = false;

            var mockPlugin = new Mock<IHealthCheckPlugin>();

            var mockStatus = new Mock<IHealthStatus>();
            mockStatus.SetupGet(s => s.Status).Returns(CheckResult.Error);
            mockPlugin.Setup(p => p.Execute()).Returns(mockStatus.Object);

            var job = new HealthCheckJob
            {
                Plugin = mockPlugin.Object
            };

            var mockList = new Mock<IStatusListener>();
            mockList.SetupGet(l => l.Threshold).Returns(CheckResult.Warning);
            mockList.Setup(l => l.Process(It.IsAny<IHealthStatus>())).Callback(() =>
            {
                executed = true;
            });

            job.Listeners.Add(mockList.Object);

            // Act
            job.Execute(null);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void NotifyListener_Should_CallListener_When_ThreshholdIsSameThreshold()
        {
            // Arrange
            var executed = false;

            var mockPlugin = new Mock<IHealthCheckPlugin>();

            var mockStatus = new Mock<IHealthStatus>();
            mockStatus.SetupGet(s => s.Status).Returns(CheckResult.Error);
            mockPlugin.Setup(p => p.Execute()).Returns(mockStatus.Object);

            var job = new HealthCheckJob
            {
                Plugin = mockPlugin.Object
            };

            var mockList = new Mock<IStatusListener>();
            mockList.SetupGet(l => l.Threshold).Returns(CheckResult.Error);
            mockList.Setup(l => l.Process(It.IsAny<IHealthStatus>())).Callback(() =>
            {
                executed = true;
            });

            job.Listeners.Add(mockList.Object);

            // Act
            job.Execute(null);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void NotifyListener_Should_NotCallListener_When_ThreshholdIsUnderThreshold()
        {
            // Arrange
            var executed = false;

            var mockPlugin = new Mock<IHealthCheckPlugin>();

            var mockStatus = new Mock<IHealthStatus>();
            mockStatus.SetupGet(s => s.Status).Returns(CheckResult.Success);
            mockPlugin.Setup(p => p.Execute()).Returns(mockStatus.Object);

            var job = new HealthCheckJob
            {
                Plugin = mockPlugin.Object
            };

            var mockList = new Mock<IStatusListener>();
            mockList.SetupGet(l => l.Threshold).Returns(CheckResult.Error);
            mockList.Setup(l => l.Process(It.IsAny<IHealthStatus>())).Callback(() =>
            {
                executed = true;
            });

            job.Listeners.Add(mockList.Object);

            // Act
            job.Execute(null);

            // Assert
            Assert.False(executed);
        }

        [Fact]
        public void NotifyListeners_Should_ContinueToCallListeners_When_AnExceptionOccursInOneListener()
        {
            // Arrange
            var executed = false;

            var mockPlugin = new Mock<IHealthCheckPlugin>();

            var mockStatus = new Mock<IHealthStatus>();
            mockStatus.SetupGet(s => s.Status).Returns(CheckResult.Success);
            mockPlugin.Setup(p => p.Execute()).Returns(mockStatus.Object);

            var job = new HealthCheckJob
            {
                Plugin = mockPlugin.Object
            };

            var mockListener1 = new Mock<IStatusListener>();
            mockListener1.Setup(l => l.Process(It.IsAny<IHealthStatus>())).Throws(new Exception("BOOM!"));
            job.Listeners.Add(mockListener1.Object);

            var mockListener2 = new Mock<IStatusListener>();
            mockListener2.Setup(l => l.Process(It.IsAny<IHealthStatus>())).Callback(() => { executed = true; });
            job.Listeners.Add(mockListener2.Object);

            // Act
            job.Execute(null);

            // Assert
            Assert.True(executed);
        }
    }
}
