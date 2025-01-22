using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DbUtiller
{
  public class Application
  {

    private IConfiguration config;

    public Application()
    {
      ParseAndLoadSettings();

      if (this.AppSettings == null)
      {
        throw new InvalidOperationException("AppSettings has not been loaded.");
      }

      // Configure Serilog
      if (config == null)
      {
        throw new InvalidOperationException("Configuration has not been loaded.");
      }

      Log.Logger = new LoggerConfiguration()
          .ReadFrom.Configuration(config)
          .CreateLogger();

      this.AccessApp = new AccessApp(this.AppSettings, Log.Logger);

      Utils.Cleanup(this.AppSettings);
    }

    public AppSettingsConfig AppSettings{ get; set; }
    public AccessApp AccessApp { get; set; }

    public async Task<int> Run(string[] args)
    {
      //Captures all the command line arguments and parses them into the Options class

      try 
      {
        Log.Information("------- Application Name: {ApplicationName} started on {0} ------", this.AppSettings.AppSettings.ApplicationName, DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"));
        await Parser.Default.ParseArguments<
         PurgeOptions,
         PruneOptions,
         PackOptions,
         PropagateOptions,
         PreserveOptions,
         ProgramOptions>(args)
           .MapResult(
               async (ProgramOptions opts) => await RunProgram(opts),
               async (PurgeOptions opts) => await AccessApp.RunPurge(opts),
               async (PruneOptions opts) => await AccessApp.RunPrune(opts),
               async (PackOptions opts) => await AccessApp.RunPack(opts),
               async (PropagateOptions opts) => await AccessApp.RunPropagate(opts),
               async (PreserveOptions opts) => await AccessApp.RunPreserve(opts),
               errs => Task.FromResult(Utils.HandleErrors(errs))
           );
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "Host terminated unexpectedly");
      }

      return 0;
    }

    // Parse and load the settings from the appsettings.json file
    // If the file does not exist, load the embedded resource
    private void ParseAndLoadSettings()
    {
      try
      {
        string tempFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        string jsonContent = string.Empty;

        if (!File.Exists(tempFilePath))
        {
          string resourceName = "DbUtiller.Resources.appsettings.json";

          // Extract the embedded resource
          jsonContent = Utils.LoadEmbeddedResource(resourceName);
        }
        else
        {
          jsonContent = File.ReadAllText(tempFilePath);
        }

        // Add the appsettings.json into the configuration builder
        config = Utils.LoadConfigurationFromJson(jsonContent);

        // Wrap the configuration settings with the AppSettingsConfig class
        this.AppSettings = new AppSettingsConfig
        {
          AppSettings = new AppSettings
          {
            ApplicationName = "DbUtiller",
            Version = "1.0.0",
            Environment = "Development"
          },
          Defaults = new DefaultsConfig(),
          Actions = new List<ActionConfig>(),
          Databases = new List<DatabaseConfig>()
        };
        config.Bind(this.AppSettings);

        Utils.InjectDefaults(this.AppSettings);
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "Host terminated unexpectedly");
      }
    }

    private static Task<int> RunProgram(ProgramOptions opts)
    {
      if (opts.settings)
      {
        using (var appSettingsEditor = new AppSettingsEditorForm(Utils.GetAppSettingsJson()))
        {
          DialogResult result = appSettingsEditor.ShowDialog();
          if (result == DialogResult.OK)
          {
            Console.WriteLine("File saved successfully!");
          }
          else
          {
            Console.WriteLine("File save cancelled.");
          }
        }
      }

      if  (opts.resources)
      {
        Utils.GetResources();
      }

      return Task.FromResult(0);
    }
  }
}