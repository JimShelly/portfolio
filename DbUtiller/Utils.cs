using System;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.Reflection;
using CommandLine;
using Microsoft.Extensions.Configuration;
using accessDB = Microsoft.Office.Interop.Access;
using accessDAO = Microsoft.Office.Interop.Access.Dao;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DbUtiller
{
  public static class Utils
  {
    // Gets the appsettings.json from the resources and extracts it to the current directory
    public static string LoadEmbeddedResource(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(resourceName);

      if (stream == null)
      {
        throw new FileNotFoundException($"Resource {resourceName} not found.");
      }

      using var reader = new StreamReader(stream);
      return reader.ReadToEnd();
    }

    public static IConfiguration LoadConfigurationFromJson(string jsonContent)
    {
      // Create a memory stream for the JSON content
      using var stream = new MemoryStream();
      using var writer = new StreamWriter(stream);
      writer.Write(jsonContent);
      writer.Flush();
      stream.Position = 0; // Reset stream position to the beginning

      // Load the JSON content into IConfiguration
      var configurationBuilder = new ConfigurationBuilder();
      configurationBuilder.AddJsonStream(stream);
      return configurationBuilder.Build();
    }

    public static string GetAppSettingsJson() {
      string tempFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
      string jsonContent = string.Empty;

      if (!File.Exists(tempFilePath))
      {
        string resourceName = "DbUtiller.Resources.appsettings.json";

        // Extract the embedded resource
        jsonContent = LoadEmbeddedResource(resourceName);
      }
      else
      {
        jsonContent = File.ReadAllText(tempFilePath);
      }

      return jsonContent;
    }

    public static int HandleErrors(IEnumerable<Error> errs)
    {
      Console.WriteLine("Error parsing arguments.");
      return 1;
    }

    public static void GetResources()
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      string[] resourceNames = assembly.GetManifestResourceNames();

    Console.WriteLine("Embedded Resources:");
    foreach (string resourceName in resourceNames)
    {
        Console.WriteLine(resourceName);

        // Read the resource content
        using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    Console.WriteLine($"Content of {resourceName}:");
                    Console.WriteLine(content);
                }
            }
      }
    }
  }


      public static void InjectDefaults(AppSettingsConfig appSettings)
      {
        var defaults = appSettings.Defaults;

        // Inject defaults into Actions
        foreach (var action in appSettings.Actions)
        {
          action.InjectDefaults(defaults);
        }

        // Inject defaults into Databases
        foreach (var database in appSettings.Databases)
        {
          database.InjectDefaults(defaults);
        }
      }

    public static string ConvertToCommaDelimited(List<string> values)
    {
      if (values == null || values.Count == 0) return string.Empty;

      return string.Join(", ", values);
    }

    public static string? BuildWhereClause(List<Filters> filters)
    {
      try
      {
        // Build the WHERE clause dynamically
        var whereClauses = new List<string>();
        foreach (var filter in filters)
        {
          if(filter.KeyValue == null) continue;
          if (filter.KeyField == null) continue;
          string columnName = filter.KeyField;
          object value = filter.KeyValue;

          // Format the value safely
          string formattedValue = value != null ? FormatSqlValue(value) : "NULL";
          whereClauses.Add($"[{columnName}] {filter.Operator} ({formattedValue})");
        }

        string whereClause = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

        return whereClause;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
        return string.Empty;
      }

    }

    public static string GenerateRandomWord()
    {
      var random = new Random();
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
      return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Formats a value for safe use in a SQL query.
    /// </summary>
    public static string FormatSqlValue(object value)
    {
      if (value == null) return "NULL";
      //if (value is string)
      //  return $"'{value.ToString().Replace("'", "''")}'"; // Escape single quotes
      return value?.ToString() ?? string.Empty;
    }


  public static void Cleanup(AppSettingsConfig appSettings)
  {
    foreach (string f in Directory.EnumerateFiles(appSettings.Defaults.Source,"clone_*"))
    { 
        File.Delete(f);
    }
    foreach (string f in Directory.EnumerateFiles(appSettings.Defaults.Source,"temp_*"))
    {
        File.Delete(f);
    }
    foreach (string f in Directory.EnumerateFiles(appSettings.Defaults.Source,"*.laccdb"))
    {
        File.Delete(f);
    }
}

    public static void CloseAccessIfOpen()
        {
            try
            {
                // Get all processes with the name "MSACCESS"
                Process[] accessProcesses = Process.GetProcessesByName("MSACCESS");

                if (accessProcesses.Length > 0)
                {
                    Console.WriteLine("Microsoft Access is running. Attempting to close...");

                    foreach (var process in accessProcesses)
                    {
                        // Close the process
                        process.CloseMainWindow();
                        process.WaitForExit(5000); // Wait for 5 seconds for the process to exit

                        if (!process.HasExited)
                        {
                            // Forcefully terminate the process if it hasn't exited
                            process.Kill();
                        }
                    }

                    Console.WriteLine("Microsoft Access has been closed.");
                }
                else
                {
                    Console.WriteLine("Microsoft Access is not running.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
  }

public class AppSettingsEditorForm : Form
    {
      private readonly RichTextBox editor;

      public AppSettingsEditorForm(string jsonContent)
      {
        Text = "Edit appsettings.json";
        Width = 800;
        Height = 600;

        // Initialize RichTextBox for editing
        editor = new RichTextBox
        {
          Dock = DockStyle.Fill,
          Font = new System.Drawing.Font("Consolas", 10) // For better JSON readability
        };
        Controls.Add(editor);

        // Load file content into the editor
        editor.Text = jsonContent;

        // Add Save button
        var saveButton = new Button
        {
          Text = "Save",
          Dock = DockStyle.Bottom
        };
        saveButton.Click += SaveButton_Click;

        Controls.Add(saveButton);
      }

      // Handles the Save button click event.
      private void SaveButton_Click(object? sender, EventArgs e)
      {
        if (!editor.Text.IsJsonValid())
        {
          MessageBox.Show("Invalid JSON format. Please correct the JSON and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        File.WriteAllText("./appsettings.json", editor.Text);
        MessageBox.Show(
            $"File saved successfully.\nYou will need to run the program again to apply the changes.",
            "File Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

        this.DialogResult = DialogResult.OK;
      }

    }

    public static class JsonExtensions
    {
      public static bool IsJsonValid(this string json)
      {
        if (string.IsNullOrWhiteSpace(json))
          return false;

        try
        {
          using var jsonDoc = JsonDocument.Parse(json);
          return true;
        }
        catch (JsonException)
        {
          return false;
        }
      }

      public static string ToJsonString(this IConfiguration configuration)
      {
        if (configuration == null)
          return string.Empty;

        try
        {
        var jsonStr = JsonSerializer.Serialize(configuration.GetChildren(), new JsonSerializerOptions { WriteIndented = true });
        
        return jsonStr;
        }
        catch (JsonException)
        {
          return string.Empty;
        }
      }
    }

  }