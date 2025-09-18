using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig.Helpers
{
    /// <summary>
    /// Helper class for database connection operations and validations
    /// Provides centralized functionality for connection management
    /// </summary>
    public static class DatabaseConnectionHelper
    {
        /// <summary>
        /// Validates database connection parameters
        /// </summary>
        public static ValidationResult ValidateConnectionParameters(
            string databaseName, 
            ConnectionDriversConfig driverConfig, 
            string installPath = null,
            bool isInMemory = false)
        {
            var errors = new List<string>();

            // Validate database name
            if (string.IsNullOrWhiteSpace(databaseName))
                errors.Add("Database name is required");
            else if (databaseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                errors.Add("Database name contains invalid characters");
            else if (databaseName.Length > 255)
                errors.Add("Database name is too long (max 255 characters)");

            // Validate driver configuration
            if (driverConfig == null)
                errors.Add(isInMemory ? "In-memory database type is required" : "Embedded database type is required");
            else if (string.IsNullOrEmpty(driverConfig.classHandler))
                errors.Add("Driver class handler is missing");

            // Validate install path for local databases
            if (!isInMemory && !string.IsNullOrWhiteSpace(installPath))
            {
                try
                {
                    var normalizedPath = NormalizePath(installPath);
                    if (!Directory.Exists(normalizedPath))
                        errors.Add($"Install folder does not exist: {normalizedPath}");
                    else if (!HasWritePermission(normalizedPath))
                        errors.Add($"No write permission for install folder: {normalizedPath}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Invalid install path: {ex.Message}");
                }
            }

            return new ValidationResult(!errors.Any(), errors);
        }

        /// <summary>
        /// Normalizes file path by handling relative paths and special characters
        /// </summary>
        public static string NormalizePath(string path, string basePath = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // Handle relative paths
            if (path.StartsWith(".") || path.Equals("/") || path.Equals("\\"))
            {
                var baseDir = basePath ?? AppContext.BaseDirectory;
                return Path.GetFullPath(Path.Combine(baseDir, path.TrimStart('.', '/', '\\')));
            }

            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Checks if a connection name already exists
        /// </summary>
        public static bool IsConnectionNameExists(IDMEEditor editor, string connectionName, string excludeGuid = null)
        {
            if (editor?.ConfigEditor?.DataConnections == null || string.IsNullOrWhiteSpace(connectionName))
                return false;

            return editor.ConfigEditor.DataConnections.Any(c => 
                c.ConnectionName.Equals(connectionName, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrEmpty(excludeGuid) || !c.GuidID.Equals(excludeGuid, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Generates a unique connection name
        /// </summary>
        public static string GenerateUniqueConnectionName(IDMEEditor editor, string baseName, string suffix = null)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "Database";

            var proposedName = string.IsNullOrWhiteSpace(suffix) ? baseName : $"{baseName}_{suffix}";
            
            if (!IsConnectionNameExists(editor, proposedName))
                return proposedName;

            var counter = 1;
            string uniqueName;
            do
            {
                uniqueName = $"{proposedName}_{counter++}";
            } while (IsConnectionNameExists(editor, uniqueName));

            return uniqueName;
        }

        /// <summary>
        /// Creates database file path for local databases
        /// </summary>
        public static string CreateDatabaseFilePath(string folderPath, string databaseName, string extension = "db")
        {
            if (string.IsNullOrWhiteSpace(extension))
                extension = "db";

            if (!extension.StartsWith("."))
                extension = "." + extension;

            return Path.Combine(folderPath, $"{databaseName}{extension}");
        }

        /// <summary>
        /// Gets the next available connection ID
        /// </summary>
        public static int GetNextConnectionId(IDMEEditor editor)
        {
            if (editor?.ConfigEditor?.DataConnections == null || editor.ConfigEditor.DataConnections.Count == 0)
                return 1;

            return editor.ConfigEditor.DataConnections.Max(c => c.ID) + 1;
        }

        /// <summary>
        /// Checks if the user has write permission to the specified directory
        /// </summary>
        private static bool HasWritePermission(string directoryPath)
        {
            try
            {
                var testFile = Path.Combine(directoryPath, $"test_write_{Guid.NewGuid()}.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates database driver configuration
        /// </summary>
        public static ValidationResult ValidateDriverConfiguration(ConnectionDriversConfig driverConfig)
        {
            var errors = new List<string>();

            if (driverConfig == null)
            {
                errors.Add("Driver configuration is required");
                return new ValidationResult(false, errors);
            }

            if (string.IsNullOrEmpty(driverConfig.classHandler))
                errors.Add("Driver class handler is missing");

            if (string.IsNullOrEmpty(driverConfig.PackageName))
                errors.Add("Driver package name is missing");

            if (string.IsNullOrEmpty(driverConfig.ConnectionString))
                errors.Add("Driver connection string template is missing");

            return new ValidationResult(!errors.Any(), errors);
        }

        /// <summary>
        /// Creates a connection string based on driver configuration and parameters
        /// </summary>
        public static string CreateConnectionString(ConnectionDriversConfig driverConfig, string databasePath = null, string databaseName = null)
        {
            if (driverConfig?.ConnectionString == null)
                return null;

            var connectionString = driverConfig.ConnectionString;

            // Replace common placeholders
            if (!string.IsNullOrEmpty(databasePath))
                connectionString = connectionString.Replace("{FilePath}", databasePath);

            if (!string.IsNullOrEmpty(databaseName))
                connectionString = connectionString.Replace("{DatabaseName}", databaseName);

            return connectionString;
        }

        /// <summary>
        /// Represents validation result with errors
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; }
            public List<string> Errors { get; }
            public string ErrorMessage => string.Join("; ", Errors);

            public ValidationResult(bool isValid, List<string> errors = null)
            {
                IsValid = isValid;
                Errors = errors ?? new List<string>();
            }
        }
    }
}