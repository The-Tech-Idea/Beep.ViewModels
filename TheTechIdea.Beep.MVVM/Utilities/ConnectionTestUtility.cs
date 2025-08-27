using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;
using System;
using System.Data;
using System.Threading.Tasks;

namespace TheTechIdea.Beep.MVVM.Utilities
{
    /// <summary>
    /// Utility class for testing database connections
    /// </summary>
    public static class ConnectionTestUtility
    {
        /// <summary>
        /// Tests a database connection using the provided connection properties
        /// </summary>
        /// <param name="editor">The DME editor instance</param>
        /// <param name="connection">The connection properties to test</param>
        /// <returns>A tuple containing success status and message</returns>
        public static (bool success, string message) TestConnection(IDMEEditor editor, ConnectionProperties connection)
        {
            if (connection == null)
            {
                return (false, "Connection properties are null");
            }

            if (editor == null)
            {
                return (false, "Editor instance is null");
            }

            try
            {
                var dataSource = editor.CreateNewDataSourceConnection(connection, connection.ConnectionName);
                if (dataSource != null)
                {
                    var connectionState = dataSource.Openconnection();
                    if (connectionState == ConnectionState.Open)
                    {
                        dataSource.Closeconnection();
                        return (true, "Connection test successful");
                    }
                    else
                    {
                        return (false, $"Connection test failed - {connectionState}");
                    }
                }
                else
                {
                    return (false, "Connection test failed - could not create data source");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Connection test failed - {ex.Message}");
            }
        }

        /// <summary>
        /// Tests a database connection asynchronously
        /// </summary>
        /// <param name="editor">The DME editor instance</param>
        /// <param name="connection">The connection properties to test</param>
        /// <returns>A task containing a tuple with success status and message</returns>
        public static async Task<(bool success, string message)> TestConnectionAsync(IDMEEditor editor, ConnectionProperties connection)
        {
            if (connection == null)
            {
                return (false, "Connection properties are null");
            }

            if (editor == null)
            {
                return (false, "Editor instance is null");
            }

            try
            {
                var dataSource = editor.CreateNewDataSourceConnection(connection, connection.ConnectionName);
                if (dataSource != null)
                {
                    var connectionState = await Task.Run(() => dataSource.Openconnection());
                    if (connectionState == ConnectionState.Open)
                    {
                        dataSource.Closeconnection();
                        return (true, "Connection test successful");
                    }
                    else
                    {
                        return (false, $"Connection test failed - {connectionState}");
                    }
                }
                else
                {
                    return (false, "Connection test failed - could not create data source");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Connection test failed - {ex.Message}");
            }
        }

        /// <summary>
        /// Tests a connection and logs the result
        /// </summary>
        /// <param name="editor">The DME editor instance</param>
        /// <param name="connection">The connection properties to test</param>
        /// <param name="databaseType">The database type for logging purposes</param>
        public static void TestConnectionWithLogging(IDMEEditor editor, ConnectionProperties connection, string databaseType)
        {
            var (success, message) = TestConnection(editor, connection);

            if (success)
            {
                editor.AddLogMessage("Beep", $"{databaseType} connection test successful", DateTime.Now, -1, null, Errors.Ok);
            }
            else
            {
                editor.AddLogMessage("Beep", $"{databaseType} {message}", DateTime.Now, -1, null, Errors.Failed);
            }
        }
    }
}
