using System;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using HealthCheck;
using HealthCheck.Framework;
using Moq;
using NLog;
using Quartz.Impl.Triggers;
using Xunit;

namespace UnitTests.Framework
{
    [SuppressMessage(
         "StyleCop.CSharp.DocumentationRules",
         "SA1600:ElementsMustBeDocumented",
         Justification = "Test Suites do not need XML Documentation.")]
    public class ComponentFactoryTests
    {
        public ComponentFactoryTests()
        {
            // Need to copy the Unit Test DLL so that MEF can find the exports below

            if (!Directory.Exists("plugins"))
            {
                Directory.CreateDirectory("plugins");
            }

            File.Copy("UnitTests.dll", "plugins\\TestPlugin.dll", true);
        }

        [Fact]
        public void Container_Should_ReturnFactory_When_PluginDirectoryDoesNotHaveFiles()
        {
            // Arrange
            File.Delete("plugins/TestPlugin.dll");

            // Act
            var factory = new ComponentFactory();

            // Assert
            Assert.NotNull(factory);
        }

        [Fact]
        public void DisposeResources_Should_DisposeContainer_When_DisposingIsTrue()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act
            var listener1 = factory.GetListener("Listen1");

            factory.Dispose(true);

            // Assert
            Assert.IsType<DummyListener1>(listener1);
            Assert.Throws<NullReferenceException>(() =>
            {
                _ = factory.GetListener("Listen2");
            });
        }

        [Fact]
        public void DisposeResources_Should_NotDisposeContainer_When_DisposingIsFalse()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act
            var listener1 = factory.GetListener("Listen1");

            factory.Dispose(false);

            var listener2 = factory.GetListener("Listen2");

            // Assert
            Assert.IsType<DummyListener1>(listener1);
            Assert.IsType<DummyListener2>(listener2);
        }

        [Fact]
        public void GetListener_Should_BeAbleToInstantiateTwoInstances()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act
            var listener1 = factory.GetListener("Listen1");
            var listener2 = factory.GetListener("Listen1");

            listener1.Threshold = CheckResult.Success;
            listener2.Threshold = CheckResult.Error;

            // Assert
            Assert.NotEqual(listener1.Threshold, listener2.Threshold);
        }

        [Fact]
        public void GetListener_Should_InstantiateTheNamedPlugin_When_AskedByContractName()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act
            var listener = factory.GetListener("Listen1");

            // Assert
            Assert.IsType<DummyListener1>(listener);
        }

        [Fact]
        public void GetListener_Should_LoadNamedInstance_When_SpecifyingByName()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act
            var listener1 = factory.GetListener("Listen1");
            var listener2 = factory.GetListener("Listen2");

            // Assert
            Assert.IsType<DummyListener1>(listener1);
            Assert.IsType<DummyListener2>(listener2);
        }

        [Fact]
        public void GetListener_Should_ReturnNull_When_ListenerTypeDoesNotExist()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act
            var listener = factory.GetListener("NonExistentListener");

            // Assert
            Assert.Null(listener);
        }

        [Fact]
        public void GetListener_Should_ThrowInvalidArgumentException_When_TypeNameIsEmpty()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = factory.GetListener(String.Empty);
            });
        }

        [Fact]
        public void GetPlugin_Should_BeAbleToInstantiateTwoInstances()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act
            var plugin1 = factory.GetPlugin("ContractName1");
            var plugin2 = factory.GetPlugin("ContractName1");

            plugin1.Name = "Plugin1";
            plugin2.Name = "Plugin2";

            // Assert
            Assert.IsType<DummyPlugin1>(plugin1);
            Assert.IsType<DummyPlugin1>(plugin2);
            Assert.NotEqual(plugin1.Name, plugin2.Name);
        }

        [Fact]
        public void GetPlugin_Should_LoadNamedInstance_When_SpecifyingByName()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act
            var plugin1 = factory.GetPlugin("ContractName1");
            var plugin2 = factory.GetPlugin("ContractName2");

            // Assert
            Assert.IsType<DummyPlugin1>(plugin1);
            Assert.IsType<DummyPlugin2>(plugin2);
        }

        [Fact]
        public void GetPlugin_Should_ReturnNull_When_PluginTypeDoesNotExist()
        {
            // Arrange
            var factory = new ComponentFactory("plugins");

            // Act
            var plugin = factory.GetPlugin("NonExistentPlugin");

            // Assert
            Assert.Null(plugin);
        }

        [Fact]
        public void GetPlugin_Should_ThrowInvalidArgumentException_When_TypeNameIsEmpty()
        {
            // Arrange
            var factory = new ComponentFactory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = factory.GetPlugin(String.Empty);
            });
        }

        [Fact]
        public void GetTrigger_Should_CreateCronTrigger()
        {
            // Arrange
            var xml = XElement.Parse("<Trigger Type='cron' Expression='0 30 4 * * ?'/>");
            var componentFactory = new ComponentFactory();

            // Act
            var trigger = componentFactory.GetTrigger(xml);

            // Assert
            Assert.IsType<CronTriggerImpl>(trigger);
        }

        [Fact]
        public void GetTrigger_Should_CreateSimpleTrigger()
        {
            // Arrange
            var xml = XElement.Parse("<Trigger Type='simple' Repeat='0:0:10'/>");
            var componentFactory = new ComponentFactory();

            // Act
            var trigger = componentFactory.GetTrigger(xml);

            // Assert
            Assert.IsType<SimpleTriggerImpl>(trigger);
        }

        [Fact]
        public void GetTrigger_Should_ReturnNull_When_CreatingCronTriggerWithNoExpression()
        {
            // Arrange
            var xml = XElement.Parse("<Trigger Type='cron'/>");
            var componentFactory = new ComponentFactory();

            // Act
            var trigger = componentFactory.GetTrigger(xml);

            // Assert
            Assert.Null(trigger);
        }

        [Fact]
        public void GetTrigger_Should_ReturnNull_When_CreatingSimpleTriggerWithNoRepeat()
        {
            // Arrange
            var xml = XElement.Parse("<Trigger Type='simple' />");
            var componentFactory = new ComponentFactory();

            // Act
            var trigger = componentFactory.GetTrigger(xml);

            // Assert
            Assert.Null(trigger);
        }

        [Fact]
        public void GetTrigger_Should_ReturnNull_When_ProvidedCronTriggerWithMisspelledExpression()
        {
            // Arrange
            var xml = XElement.Parse("<Trigger Type='cron' Expresion='* * * * * *' />");
            var componentFactory = new ComponentFactory();

            // Act
            var trigger = componentFactory.GetTrigger(xml);

            // Assert
            Assert.Null(trigger);
        }

        [Fact]
        public void GetTrigger_Should_ReturnNull_When_ProvidedNullTrigger()
        {
            // Arrange
            var componentFactory = new ComponentFactory();

            // Act
            var trigger = componentFactory.GetTrigger(null);

            // Assert
            Assert.Null(trigger);
        }

        [Fact]
        public void GetTrigger_Should_ReturnNull_When_ProvidedUnknownTrigger()
        {
            // Arrange
            var xml = XElement.Parse("<Trigger Type = 'invalid' />");
            var componentFactory = new ComponentFactory();

            // Act
            var trigger = componentFactory.GetTrigger(xml);

            // Assert
            Assert.Null(trigger);
        }

        [Fact]
        public void GetTrigger_Should_ReturnNull_When_ProvidedXMLMistingTypeOfTrigger()
        {
            // Arrange
            var xml = XElement.Parse("<Trigger />");
            var componentFactory = new ComponentFactory();

            // Act
            var trigger = componentFactory.GetTrigger(xml);

            // Assert
            Assert.Null(trigger);
        }

        [Fact]
        public void GetTrigger_Should_ThrowException_When_ProvidedInvalidTimespan()
        {
            // Arrange
            var xml = XElement.Parse("<Trigger Type='simple' Repeat='0:0:10x'/>");
            var componentFactory = new ComponentFactory();

            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                _ = componentFactory.GetTrigger(xml);
            });
        }

        [Export("Listen1", typeof(IStatusListener))]
        public class DummyListener1 : IStatusListener
        {
            private static readonly Logger _log = LogManager.GetCurrentClassLogger();

            public CheckResult Threshold { get; set; }

            public void Initialize()
            {
                _log.Debug("Initialize");
            }

            public bool Process(IHealthStatus status)
            {
                _log.Warn(status.Status);

                return true;
            }
        }

        [Export("Listen2", typeof(IStatusListener))]
        public class DummyListener2 : IStatusListener
        {
            private static readonly Logger _log = LogManager.GetCurrentClassLogger();

            public CheckResult Threshold { get; set; }

            public void Initialize()
            {
                _log.Debug("Initialize");
            }

            public bool Process(IHealthStatus status)
            {
                _log.Warn(status.Status);

                return true;
            }
        }

        // [PartCreationPolicy(CreationPolicy.NonShared)]
        [Export("ContractName1", typeof(IHealthCheckPlugin))]
        public class DummyPlugin1 : IHealthCheckPlugin
        {
            private static readonly Logger _log = LogManager.GetCurrentClassLogger();

            public string GroupName { get; set; }

            public string Name { get; set; }
            public PluginStatus PluginStatus { get; set; }

            public IHealthStatus Execute()
            {
                return new Mock<IHealthStatus>().Object;
            }

            public void SetTaskConfiguration(XElement configurationElement)
            {
                _log.Debug("SettaskConfiguration");
            }

            public void Shutdown()
            {
                _log.Debug("Shutdown");
            }

            public void Startup()
            {
                _log.Debug("Startup");
            }
        }

        [Export("ContractName2", typeof(IHealthCheckPlugin))]
        public class DummyPlugin2 : IHealthCheckPlugin
        {
            private static readonly Logger _log = LogManager.GetCurrentClassLogger();

            public string GroupName { get; set; }

            public string Name { get; set; }

            public PluginStatus PluginStatus { get; set; }

            public IHealthStatus Execute()
            {
                return new Mock<IHealthStatus>().Object;
            }

            public void SetTaskConfiguration(XElement configurationElement)
            {
                _log.Debug("SettaskConfiguration");
            }

            public void Shutdown()
            {
                _log.Debug("Shutdown");
            }

            public void Startup()
            {
                _log.Debug("Startup");
            }
        }

        [Export("Namespace.ContractName2", typeof(IHealthCheckPlugin))]
        public class DummyPlugin3 : IHealthCheckPlugin
        {
            private static readonly Logger _log = LogManager.GetCurrentClassLogger();

            public string GroupName { get; set; }

            public string Name { get; set; }

            public PluginStatus PluginStatus { get; set; }

            public IHealthStatus Execute()
            {
                return new Mock<IHealthStatus>().Object;
            }

            public void SetTaskConfiguration(XElement configurationElement)
            {
                _log.Debug("SettaskConfiguration");
            }

            public void Shutdown()
            {
                _log.Debug("Shutdown");
            }

            public void Startup()
            {
                _log.Debug("Startup");
            }
        }

        [Export("Namespace.ContractName3", typeof(IHealthCheckPlugin))]
        public class DummyPlugin4 : IHealthCheckPlugin
        {
            private static readonly Logger _log = LogManager.GetCurrentClassLogger();

            public string GroupName { get; set; }

            public string Name { get; set; }

            public PluginStatus PluginStatus { get; set; }

            public IHealthStatus Execute()
            {
                return new Mock<IHealthStatus>().Object;
            }

            public void SetTaskConfiguration(XElement configurationElement)
            {
                _log.Debug("SettaskConfiguration");
            }

            public void Shutdown()
            {
                _log.Debug("Shutdown");
            }

            public void Startup()
            {
                _log.Debug("Startup");
            }
        }

        [Export("Namespace.ContractName3", typeof(IHealthCheckPlugin))]
        public class DummyPlugin5 : IHealthCheckPlugin
        {
            private static readonly Logger _log = LogManager.GetCurrentClassLogger();

            public string GroupName { get; set; }

            public string Name { get; set; }

            public PluginStatus PluginStatus { get; set; }

            public IHealthStatus Execute()
            {
                return new Mock<IHealthStatus>().Object;
            }

            public void SetTaskConfiguration(XElement configurationElement)
            {
                _log.Debug("SettaskConfiguration");
            }

            public void Shutdown()
            {
                _log.Debug("Shutdown");
            }

            public void Startup()
            {
                _log.Debug("Startup");
            }
        }
    }
}
