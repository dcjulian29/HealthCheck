using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using HealthCheck.Framework;
using Xunit;

namespace UnitTests
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
                { }
            }

            Directory.CreateDirectory("conf");
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
        public void Read_Should_ReturnNoChecks_When_ProvidedXmlHasNoChecks()
        {
            // Arrange
            File.Copy("EmptyHealthChecks.xml", "conf\\empty.xml");
            var reader = new ConfigurationFileReader();

            // Act
            var groups = reader.Read("conf\\empty.xml");

            // Assert
            Assert.Empty(groups[0].Checks);
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
        public void Read_Should_ThrowException_When_ProvidedDuplicateChecks()
        {
            // Arrange
            File.Copy("DuplicateCheck.xml", "conf\\DuplicateCheck.xml");
            var reader = new ConfigurationFileReader();

            // Act & Assert
            Assert.Throws<ApplicationException>(() =>
            {
                var groups = reader.ReadAll();
            });
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
            Assert.Equal(0, groups.Count);
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
        public void ReadAll_Should_ThrowException_When_ProvidedDuplicateGroups()
        {
            // Arrange
            File.Copy("ValidConfig1.xml", "conf\\valid1.xml");
            File.Copy("ValidConfig1.xml", "conf\\valid2.xml");
            var reader = new ConfigurationFileReader();

            // Act & Assert
            Assert.Throws<ApplicationException>(() =>
            {
                var groups = reader.ReadAll();
            });
        }
    }
}
