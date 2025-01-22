using accessDB = Microsoft.Office.Interop.Access;
using accessDAO = Microsoft.Office.Interop.Access.Dao;
using Serilog;
using System.Text.RegularExpressions;

namespace DbUtiller
{
    public class AccessDatabase
    {
        private readonly Serilog.ILogger _logger;
        public AccessDatabase(
            string Name, 
            Enums.Action Action, 
            ActionConfig actionConfig, 
            DatabaseConfig databaseConfig,
            ILogger _logger
        )
        {
            this.dbName = Name;
            this.actionType = Action;
            this.actionConfig = actionConfig;
            this.dbConfig = databaseConfig;
            this.dbConfig.ActionConfig = actionConfig;
            this._logger = _logger;
        }

        private string dbName { get; set; }
        private Enums.Action actionType { get; set; }
        public ActionConfig actionConfig { get; set; }
        private DatabaseConfig dbConfig { get; set; }

        #region Tasks
        public async Task<int> PropagateTask()
        {

            Console.WriteLine("Propagate Started");
            Log.Information("Propagate Task Started");
            CloneDatabase(dbConfig.SourcePathAndName, dbConfig.TargetPathAndName);
            
            return await Task.FromResult<int>(0);
        }

        public async Task<int> PreserveTask()
        {
            Log.Information("Preserve Task Started");
            BackupDatabase(dbConfig.SourcePathAndName, dbConfig.TargetPathAndName);

            return await Task.FromResult<int>(0);
        }

        public async Task<int> PurgeTask(string purgeDate)
        {

            Log.Information("Purge Task Started");

            //Initialize information
            var selectStatement = actionConfig.SelectStatement;
            Console.WriteLine("Select Statement = {0}", selectStatement);
            string pattern = @"\{\{(\w+)\}\}";
            selectStatement = Regex.Replace(selectStatement, pattern, purgeDate);
            Console.WriteLine("Select Statement = {0}", selectStatement);

            //First, get the records to delete
            Console.WriteLine("Select Statement: {0}", selectStatement);
            var searchList = GetSearchValues(dbConfig.SourcePathAndName, selectStatement);
            if(searchList.Count == 0) {
                Console.WriteLine("No records to delete");
                return await Task.FromResult<int>(0);
            }

            Console.WriteLine("Search List = {0}", Utils.ConvertToCommaDelimited(searchList));
            //Second, loop through all the tables from the appsettings.json file
            for (int i = 0; i < actionConfig.Tables.Count; i++)
            {
                string tableName = actionConfig.Tables[i].Name ?? string.Empty;
                _logger.Information("Beginning Purge for table {0}", tableName);
                //Third, build the where clause for each table from the filters section of the appsettings.json file
                var whereStatement = Utils.BuildWhereClause(new List<Filters>
                {
                    new Filters
                    {
                        KeyField = "JobNbr",
                        KeyValue = Utils.ConvertToCommaDelimited(searchList),
                        Operator = "IN"
                    }
                });
                //Fourth, transfer the records from the source to the target database
                Console.WriteLine($"{0} - Archiving {1} records.", tableName, searchList.Count);
                if (tableName != null)
                {
                    TransferRecords(dbConfig.SourcePathAndName, dbConfig.TargetPathAndName, tableName, whereStatement ?? string.Empty);
                }
                else
                {
                    Console.WriteLine("Table name is null, skipping transfer.");
                }
                //Fifth, Delete the records from the source database
                var deleteStatement = $"DELETE FROM {tableName} WHERE JobNbr IN ({Utils.ConvertToCommaDelimited(searchList)})";
                PurgeRecords(dbConfig.SourcePathAndName, deleteStatement);
            }
            //Last, compact the database
            //PackTask();
            return await Task.FromResult<int>(0);
        }

        public async Task<int> PruneTask()
        {

            Log.Information("Preserve Task Started");
            BackupDatabase(dbConfig.SourcePathAndName, dbConfig.TargetPathAndName);
            for(int i = 0; i < actionConfig.Tables.Count; i++)
            {
                int count = 0;
                Console.WriteLine("Deleting records from table: {0}", actionConfig.Tables[i].Name);
                var sqlCommand = actionConfig.Tables[i].SQLCommand;
                if (sqlCommand != null)
                {
                    count = PurgeRecords(dbConfig.SourcePathAndName, sqlCommand);
                    Console.WriteLine($"{count} records deleted");
                }
                else
                {
                    Console.WriteLine("SQL command is null, skipping purge for table: {0}", actionConfig.Tables[i].Name);
                }
                Console.WriteLine($"{count} records deleted");
            }
            return await Task.FromResult<int>(0);
        }

        public async Task<int> PackTask()
        {
            Log.Information("Pack Task Started");

           Log.Information("Compacting and repairing database: {DatabaseName}...", dbConfig.Name);
            try
            {
            // Create an instance of the Access application
            var accessApp = new accessDB.Application();

            Console.WriteLine("Source: {0}, Destination: {1}", dbConfig.SourcePathAndName, dbConfig.TargetPathAndName);
            // Compact and repair the database
            accessApp.CompactRepair(
                SourceFile: dbConfig.SourcePathAndName,
                DestinationFile: dbConfig.TargetPathAndName,
                LogFile: true
            );

            System.Threading.Thread.Sleep(1000); // Wait for 1 second

            // Replace the original database with the compacted one
            if(File.Exists(dbConfig.SourcePathAndName)) {
                System.IO.File.Delete(dbConfig.SourcePathAndName);
                Console.WriteLine($"Deleted original database: {dbConfig.SourcePathAndName}");
            }
            if(File.Exists(dbConfig.TargetPathAndName)) {
                System.IO.File.Move(dbConfig.TargetPathAndName, dbConfig.SourcePathAndName);
                Console.WriteLine($"Renamed compacted database to: {dbConfig.SourcePathAndName}");
            }


            Log.Information("Database compacted and repaired successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred");
        }

            return await Task.FromResult<int>(0);

        }
        #endregion

        #region Private Functions
        private static void BackupDatabase(string sourcePath, string backupPath)
        {
            try
            {
                Log.Information("Backing up database from {Source} to {Backup}", sourcePath, backupPath);

                var daoEngine = new accessDAO.DBEngine();

                var counter = 0;
                while(File.Exists(backupPath)) {
                    backupPath = backupPath.Replace(".accdb", $"_{counter}.accdb");
                    counter++;
                }
                // Compact the database (creates a compacted copy at the backup location)
                daoEngine.CompactDatabase(
                    sourcePath,    // Source database file
                    backupPath     // Destination file for the compacted/backup database
                );

                Log.Information("Backup completed successfully: {Backup}", backupPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to back up database: {Source}", sourcePath);
            }
        }

        public static void CloneDatabase(string sourcePath, string targetPath)
        {
            var daoEngine = new accessDAO.DBEngine();
            accessDAO.Database? backupDb = null;

            try
            {
                // Step 1: Backup the database
                if (!File.Exists(sourcePath))
                {
                    Console.WriteLine($"Source database not found: {sourcePath}");
                    return;
                }
                
                //Do not clone if the file exists. 
                if(File.Exists(targetPath))
                {
                    return;
                }

                // Copy the database to the backup location
                File.Copy(sourcePath, targetPath, overwrite: false);

                // Step 2: Open the backup database
                backupDb = daoEngine.OpenDatabase(targetPath);

                // Step 3: Delete data from all user tables
                foreach (accessDAO.TableDef table in backupDb.TableDefs)
                {
                    if(!string.IsNullOrEmpty(table.Connect)){
                        continue;
                    }

                    // Skip system tables
                    if ((table.Attributes & (int)accessDAO.TableDefAttributeEnum.dbSystemObject) != 0)
                        continue;

                    try

                    {
                        string deleteQuery = $"DELETE FROM [{table.Name}]";
                        backupDb.Execute(deleteQuery);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to clear data from table '{table.Name}': {ex.Message}");
                    }
                }

                Console.WriteLine("All table data cleared in the backup database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Ensure database is closed
                backupDb?.Close();
            }
        }

        public static int PurgeRecords(string databasePath, string sqlCommand)
        {
            try
            {
                // Initialize DAO objects
                accessDAO.DBEngine dbEngine = new accessDAO.DBEngine();
                accessDAO.Database database = dbEngine.OpenDatabase(databasePath);

                // Build the SQL DELETE statement
                string sql = sqlCommand;

                // Execute the DELETE query
                database.Execute(sql, accessDAO.RecordsetOptionEnum.dbFailOnError);
                int recordsAffected = database.RecordsAffected;

                Console.WriteLine("Record deleted successfully.");

                // Clean up
                database.Close();

                return recordsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1;
            }
        }

        public static List<string> GetSearchValues(string sourcePath, string sqlStatement)
        {
            Console.WriteLine("Source Path: {0}", sourcePath);

            accessDAO.DBEngine dbEngine = new accessDAO.DBEngine();

            // Open source and destination databases
            accessDAO.Database sourceDb = dbEngine.OpenDatabase(sourcePath);

            string sql = sqlStatement;
            
            var result = new List<string>();

            try
            {
                accessDAO.Recordset rs = sourceDb.OpenRecordset(sql, accessDAO.RecordsetTypeEnum.dbOpenSnapshot);

                while (!rs.EOF)
                {
                    object fieldValue = rs.Fields[0].Value;

                    // Add the value to the list, converting it to a string (null-safe)
                    if (fieldValue != null)
                    {
                        result.Add(fieldValue?.ToString() ?? string.Empty);
                    }

                    rs.MoveNext();
                }

                // Clean up
                rs.Close();
                sourceDb.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {0}", ex);
            }

            return result;
        }

        public static int TransferRecords(string sourcePath, string destinationPath, string table, string whereClause = "")
        {

            accessDAO.Database sourceDb = null;
            accessDAO.Database destDb = null;
            try
            {
                // Initialize DAO engine
                accessDAO.DBEngine dbEngine = new accessDAO.DBEngine();

                // Open source and destination databases
                sourceDb = dbEngine.OpenDatabase(sourcePath);
                destDb = dbEngine.OpenDatabase(destinationPath);

                // Build SQL INSERT INTO ... SELECT ... query
                string sql = $@"
                    INSERT INTO [{table}] IN '{destinationPath}'
                    SELECT * FROM [{table}] IN '{sourcePath}'
                ";


                // Add a WHERE clause if specified
                if (!string.IsNullOrEmpty(whereClause))
                {
                    sql += $" {whereClause}";
                }
                Console.WriteLine("SQL: {0}", sql);
                // Execute the query in the destination database
                sourceDb.Execute(sql, accessDAO.RecordsetOptionEnum.dbFailOnError);
                int recordsAffected = destDb.RecordsAffected;

                return recordsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return -1;
            }
            finally
            {
                // Clean up
                sourceDb?.Close();
                destDb?.Close();
            }
        }

        #endregion
    }
}