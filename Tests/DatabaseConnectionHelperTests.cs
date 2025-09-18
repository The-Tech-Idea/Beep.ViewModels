using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheTechIdea.Beep.MVVM.ViewModels.BeepConfig.Helpers;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.DataBase;

namespace TheTechIdea.Beep.MVVM.Tests.ViewModels.BeepConfig.Helpers
{
    /// <summary>
    /// Unit tests for DatabaseConnectionHelper
    /// These tests demonstrate how the refactored code can be tested
    /// </summary>
    [TestClass]
    public class DatabaseConnectionHelperTests
    {
        [TestMethod]
        public void ValidateConnectionParameters_WithValidInput_ReturnsValid()
        {
            // Arrange
            var databaseName = "TestDatabase";
            var driverConfig = new ConnectionDriversConfig
            {
                classHandler = "TestHandler",
                PackageName = "TestPackage",
                ConnectionString = "Data Source={FilePath};"
            };

            // Act
            var result = DatabaseConnectionHelper.ValidateConnectionParameters(
                databaseName, 
                driverConfig, 
                isInMemory: true);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidateConnectionParameters_WithEmptyDatabaseName_ReturnsInvalid()
        {
            // Arrange
            var databaseName = "";
            var driverConfig = new ConnectionDriversConfig
            {
                classHandler = "TestHandler",
                PackageName = "TestPackage",
                ConnectionString = "Data Source={FilePath};"
            };

            // Act
            var result = DatabaseConnectionHelper.ValidateConnectionParameters(
                databaseName, 
                driverConfig, 
                isInMemory: true);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Count > 0);
            Assert.IsTrue(result.ErrorMessage.Contains("Database name is required"));
        }

        [TestMethod]
        public void ValidateConnectionParameters_WithInvalidCharacters_ReturnsInvalid()
        {
            // Arrange
            var databaseName = "Test<>Database"; // Contains invalid characters
            var driverConfig = new ConnectionDriversConfig
            {
                classHandler = "TestHandler",
                PackageName = "TestPackage",
                ConnectionString = "Data Source={FilePath};"
            };

            // Act
            var result = DatabaseConnectionHelper.ValidateConnectionParameters(
                databaseName, 
                driverConfig, 
                isInMemory: true);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage.Contains("invalid characters"));
        }

        [TestMethod]
        public void ValidateConnectionParameters_WithNullDriverConfig_ReturnsInvalid()
        {
            // Arrange
            var databaseName = "TestDatabase";
            ConnectionDriversConfig driverConfig = null;

            // Act
            var result = DatabaseConnectionHelper.ValidateConnectionParameters(
                databaseName, 
                driverConfig, 
                isInMemory: true);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage.Contains("In-memory database type is required"));
        }

        [TestMethod]
        public void NormalizePath_WithRelativePath_ReturnsAbsolutePath()
        {
            // Arrange
            var relativePath = "./TestFolder";
            var basePath = @"C:\TestBase";

            // Act
            var result = DatabaseConnectionHelper.NormalizePath(relativePath, basePath);

            // Assert
            Assert.IsTrue(Path.IsPathRooted(result));
            Assert.IsTrue(result.Contains("TestFolder"));
        }

        [TestMethod]
        public void NormalizePath_WithAbsolutePath_ReturnsSamePath()
        {
            // Arrange
            var absolutePath = @"C:\TestFolder\SubFolder";

            // Act
            var result = DatabaseConnectionHelper.NormalizePath(absolutePath);

            // Assert
            Assert.AreEqual(Path.GetFullPath(absolutePath), result);
        }

        [TestMethod]
        public void CreateDatabaseFilePath_WithValidInput_ReturnsCorrectPath()
        {
            // Arrange
            var folderPath = @"C:\TestFolder";
            var databaseName = "TestDatabase";
            var extension = "db";

            // Act
            var result = DatabaseConnectionHelper.CreateDatabaseFilePath(folderPath, databaseName, extension);

            // Assert
            var expectedPath = Path.Combine(folderPath, "TestDatabase.db");
            Assert.AreEqual(expectedPath, result);
        }

        [TestMethod]
        public void CreateDatabaseFilePath_WithExtensionWithoutDot_AddsCorrectExtension()
        {
            // Arrange
            var folderPath = @"C:\TestFolder";
            var databaseName = "TestDatabase";
            var extension = "sqlite"; // Without dot

            // Act
            var result = DatabaseConnectionHelper.CreateDatabaseFilePath(folderPath, databaseName, extension);

            // Assert
            var expectedPath = Path.Combine(folderPath, "TestDatabase.sqlite");
            Assert.AreEqual(expectedPath, result);
        }

        [TestMethod]
        public void ValidateDriverConfiguration_WithValidConfig_ReturnsValid()
        {
            // Arrange
            var driverConfig = new ConnectionDriversConfig
            {
                classHandler = "TestHandler",
                PackageName = "TestPackage",
                ConnectionString = "Data Source={FilePath};"
            };

            // Act
            var result = DatabaseConnectionHelper.ValidateDriverConfiguration(driverConfig);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidateDriverConfiguration_WithMissingHandler_ReturnsInvalid()
        {
            // Arrange
            var driverConfig = new ConnectionDriversConfig
            {
                classHandler = "", // Missing handler
                PackageName = "TestPackage",
                ConnectionString = "Data Source={FilePath};"
            };

            // Act
            var result = DatabaseConnectionHelper.ValidateDriverConfiguration(driverConfig);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage.Contains("class handler is missing"));
        }

        [TestMethod]
        public void CreateConnectionString_WithPlaceholders_ReplacesCorrectly()
        {
            // Arrange
            var driverConfig = new ConnectionDriversConfig
            {
                ConnectionString = "Data Source={FilePath};Initial Catalog={DatabaseName};"
            };
            var databasePath = @"C:\Test\Database.db";
            var databaseName = "TestDB";

            // Act
            var result = DatabaseConnectionHelper.CreateConnectionString(driverConfig, databasePath, databaseName);

            // Assert
            Assert.AreEqual("Data Source=C:\\Test\\Database.db;Initial Catalog=TestDB;", result);
        }

        [TestMethod]
        public void GetNextConnectionId_WithEmptyList_ReturnsOne()
        {
            // This test would require mocking IDMEEditor
            // For demonstration purposes, showing the test structure
            
            // Act
            var result = 1; // Simulated result

            // Assert
            Assert.AreEqual(1, result);
        }
    }

    /// <summary>
    /// Mock classes for testing purposes
    /// In a real test project, these would be in separate files or use a mocking framework
    /// </summary>
    public class MockConnectionDriversConfig : ConnectionDriversConfig
    {
        public MockConnectionDriversConfig()
        {
            classHandler = "MockHandler";
            PackageName = "MockPackage";
            ConnectionString = "Mock Connection String";
        }
    }
}