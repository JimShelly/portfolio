using System;
using accessDB = Microsoft.Office.Interop.Access;
using accessDAO = Microsoft.Office.Interop.Access.Dao;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text;
using System.Globalization;

namespace DbUtiller
{
    public class AccessApp
    {
      private readonly ILogger _logger;
 
      public AccessApp(AppSettingsConfig appSettings, ILogger logger)
      {
        _appSettings = appSettings;
        _logger = logger;
      }

      private readonly AppSettingsConfig _appSettings;

    public async Task<int> RunPurge(PurgeOptions opts)
    {
      _logger.Information("Run Purge...");
      DateTime defaultDate = DateTime.Now.AddDays(_appSettings.Defaults.PurgeDays);

      if (!string.IsNullOrEmpty(opts.PurgeDate))
      {
        if (!DateTime.TryParse(opts.PurgeDate, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime date))
        {
          Console.WriteLine("Invalid date format. Please use 'MM-dd-yyyy'.");
          return 1;
        }
        else if (date >= DateTime.Now)
        {
          _logger.Error("Date cannot be in the future: {0}", date.ToString("MM-dd-yyyy"));
          Console.WriteLine("Date cannot be in the future.");
          return 1;
        }
        else if (DateTime.Parse(opts.PurgeDate, new CultureInfo("en-US")) > DateTime.Now.AddDays(-365))
        {
          string randomWord = Utils.GenerateRandomWord();
          Console.Write($"The date provided is less than a year old. Please type the following word to proceed: ");
          Console.BackgroundColor = ConsoleColor.Blue;
          Console.ForegroundColor = ConsoleColor.White;
          Console.Write($"{randomWord}");
          Console.ResetColor();
          Console.WriteLine();
          string userInput = Console.ReadLine() ?? string.Empty;

          while(userInput != randomWord) {
            Console.WriteLine("The word you entered does not match. Please try again.");
            userInput = Console.ReadLine() ?? string.Empty;
          }

          defaultDate = DateTime.Parse(opts.PurgeDate, new CultureInfo("en-US"));
        }
        else
        {
          defaultDate = date;
          _logger.Information("Starting Purge Process for Date {PurgeDate}!", defaultDate.ToString("MM-dd-yyyy"));

        }
      }
      else
      {
        Console.WriteLine($"No date specified. Default date {defaultDate.ToString("MM-dd-yyyy")} will be used.");
      }

      var packAction = _appSettings.Actions.FirstOrDefault(action => action.Name == "Pack");
      var purgeAction = _appSettings.Actions.FirstOrDefault(action => action.Name == "Purge");
      var propagateAction = _appSettings.Actions.FirstOrDefault(action => action.Name == "Propagate");
      var preserveAction = _appSettings.Actions.FirstOrDefault(action => action.Name == "Preserve");
Console.WriteLine("Purge Action: {0} - {1}", purgeAction.Description, purgeAction.SelectStatement);

      // Get all databases or the specified database from appsettings
      List<DatabaseConfig> dbConfig = GetDatabases(opts);

      if (dbConfig.Count == 0)
      {
        _logger.Error(Constants.Logs.NoDatabasesFoundMessage);
        return -1;
      }

      foreach (var db in dbConfig)
      {

        try
        {
          //After the backup and once we've cloned the database, we can rename the clone 
          _logger.Information("purgeAction?.Description: {0}", db.Name);

          if (purgeAction == null)
          {
            _logger.Error("Purge action configuration is missing.");
            return -1;
          }
          if (string.IsNullOrEmpty(db.Name))
          {
            _logger.Error(Constants.Logs.DatabaseNameMissingMessage);
            return -1;
          }

          AccessDatabase database = new AccessDatabase(db.Name, Enums.Action.Purge, purgeAction, db, _logger);
          string cloneName = string.Empty;
          var originalFileInfo = new FileInfo(db.SourcePathAndName);

          //_logger.Information("\tBackup Database: {0}", db.Name);
          //db.ActionConfig = preserveAction;
          //await database.PreserveTask();
          //_logger.Information("\tCreate a template of the database: {0}", db.Name);
          db.ActionConfig = propagateAction;
          cloneName = db.TargetPathAndName;
          await database.PropagateTask();
          _logger.Information("\tPurge the database: {0}", db.Name);
          db.ActionConfig = purgeAction;
          Console.WriteLine("cloneName: {0}, {1}", cloneName, db.TargetPathAndName);
          if(!File.Exists(db.TargetPathAndName)) {
            File.Move(cloneName, db.TargetPathAndName);
          }
          await database.PurgeTask(defaultDate.ToString("MM/dd/yyyy"));
          Thread.Sleep(3000);
          _logger.Information("\tPack the database: {0}", db.Name);
          //db.ActionConfig = packAction;
          //await database.PackTask();
          //Thread.Sleep(1000);
          //var purgedFileInfo = new FileInfo(db.SourcePathAndName);
          //Console.WriteLine("{0}: Original file size: {1} bytes; Purged file size: {2} bytes.", originalFileInfo.Name, originalFileInfo.Length, purgedFileInfo.Length);

          Log.Logger.Information("Database purged successfully.");
        }
        catch (Exception ex)
        {
          Log.Error(ex, Constants.Logs.ErrorMessage);
        }
      }

      return 0;
    }

    public async Task<int> RunPrune(PruneOptions opts)
    {
      var pruneAction = _appSettings.Actions.FirstOrDefault(action => action.Name == "Prune");

      // Get all databases or the specified database from appsettings
      List<DatabaseConfig> dbConfig = GetDatabases(opts);

      if (dbConfig.Count == 0)
      {
        Log.Logger.Error(Constants.Logs.NoDatabasesFoundMessage);
        return -1;
      }

      foreach (var db in dbConfig)
      {

        try
        {
          if (pruneAction != null)
          {
            _logger.Information("{Description}: {DatabaseName}", pruneAction.Description, db.Name);
          }
          else
          {
            _logger.Error("Prune action configuration is missing.");
            return -1;
          }
          if (string.IsNullOrEmpty(db.Name))
          {
            _logger.Error("Database name is missing.");
            return -1;
          }
          AccessDatabase database = new AccessDatabase(db.Name, Enums.Action.Propagate, pruneAction, db, _logger);

          await database.PruneTask();

          _logger.Information("Database backup completed successfully.");
        }
        catch (Exception ex)
        {
          _logger.Error(ex, Constants.Logs.ErrorMessage);
        }
      }

      return 0;
    }

    public async Task<int> RunPack(PackOptions opts)
    {
      _logger.Information("Run Pack...");
      var packAction = _appSettings.Actions.FirstOrDefault(action => action.Name == "Pack");

      List<DatabaseConfig> dbConfig = GetDatabases(opts);
      if (dbConfig == null || dbConfig.Count == 0)
      {
        _logger.Error(Constants.Logs.NoDatabasesFoundMessage);
       return -1;
      }

      foreach (var db in dbConfig)
      {

        try
        {
          if (packAction != null)
          {
            Console.WriteLine($"{packAction.Description}: {db.Name}");
          }
          else
          {
            _logger.Error("Pack action configuration is missing.");
            return -1;
          }
          if (string.IsNullOrEmpty(db.Name))
          {
            _logger.Error("Database name is missing.");
            return -1;
          }
          AccessDatabase database = new AccessDatabase(db.Name, Enums.Action.Pack, packAction, db, _logger);

          await database.PackTask();
      

          Log.Information("Database compacted and repaired successfully.");
        }
        catch (Exception ex)
        {
          Log.Error(ex, "An error occurred");
        }
      }

      return 0;
    }

    public async Task<int> RunPropagate(PropagateOptions opts)
    {
      var propagateAction = _appSettings.Actions.FirstOrDefault(action => action.Name == "Propagate");

      // Get all databases or the specified database from appsettings
      List<DatabaseConfig> dbConfig = GetDatabases(opts);

      if (dbConfig == null || dbConfig.Count == 0)
      {
        Console.WriteLine("No databases found in appsettings.json");
        return 1;
      }

      //Iterate over the databases and make a template of each database
      foreach (var db in dbConfig)
      {

        try
        {
          if (propagateAction != null)
          {
            _logger.Information("{Description}: {DatabaseName}", propagateAction.Description, db.Name);
          }
          else
          {
            _logger.Error("Propagate action configuration is missing.");
            return -1;
          }
          if (string.IsNullOrEmpty(db.Name))
          {
            _logger.Error("Database name is missing.");
            return -1;
          }
          AccessDatabase database = new AccessDatabase(db.Name, Enums.Action.Propagate, propagateAction, db, _logger);

          await database.PropagateTask();


          Log.Information("Database backup completed successfully.");
        }
        catch (Exception ex)
        {
          Log.Error(ex, "An error occurred");
        }
      }
      return 0;
    }

    public async Task<int> RunPreserve(PreserveOptions opts)
    {
      var preserveAction = _appSettings.Actions.FirstOrDefault(action => action.Name == "Preserve");

      // Get all databases or the specified database from appsettings
      List<DatabaseConfig> dbConfig = GetDatabases(opts);

      if (dbConfig == null || dbConfig.Count == 0)
      {
        Console.WriteLine("No databases found in appsettings.json");
        return 1;
      }

      var dbNames = dbConfig.Select(db => db.Name);

      foreach (var dbName in dbNames)
      {
        try
        {
          if (preserveAction != null)
          {
            Console.WriteLine($"{preserveAction.Description}: {dbName}");
          }
          else
          {
            _logger.Error("Preserve action configuration is missing.");
            return -1;
          }
          if (dbName == null)
          {
            _logger.Error("Database name is null.");
            return -1;
          }
          AccessDatabase database = new AccessDatabase(dbName, Enums.Action.Pack, preserveAction, dbConfig.First(db => db.Name == dbName), _logger);

          await database.PreserveTask();

          _logger.Information("Database backup completed successfully.");
        }
        catch (Exception ex)
        {
          _logger.Error(ex, "An error occurred");
        }
      }
      return 0;
    }

    #region Private Methods
      private List<DatabaseConfig> GetDatabases(GlobalOptions opts)
      {
        List<DatabaseConfig> dbConfig = _appSettings.Databases;
        if (opts.Name != null)
        {
          var dbConfigItem = _appSettings.Databases.FirstOrDefault(db => db.Name == opts.Name);
          dbConfig = dbConfigItem != null ? new List<DatabaseConfig> { dbConfigItem } : new List<DatabaseConfig>();
        }

      return dbConfig;
    }
    #endregion
  }
}