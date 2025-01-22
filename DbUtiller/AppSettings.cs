using System;
using System.Diagnostics;
using HandlebarsDotNet;

namespace DbUtiller
{
  public class AppSettingsConfig
  {
    public required AppSettings AppSettings { get; set; }
    public Logging? Logging { get; set; }
    public required DefaultsConfig Defaults { get; set; }
    public required List<ActionConfig> Actions { get; set; }
    public required List<DatabaseConfig> Databases { get; set; }
  }

  public class AppSettings
  {
    public required string ApplicationName { get; set; }
    public required string Version { get; set; }
    public required string Environment { get; set; }
    public List<String>? SupportEmails { get; set; }
  }

  public class Logging
  {
    public LogLevel? LogLevel { get; set; }
  }

  public class LogLevel
  {
    public string? Default { get; set; }
    public string? System { get; set; }
    public string? Microsoft { get; set; }
  }

  public class DefaultsConfig
  {
    public string? FileNameTemplate { get; set; }
    public string? DateFormat { get; set; }
    public int PurgeDays { get; set; }
    public string? Source { get; set; }
    public string? Target { get; set; }
    public string? SelectStatement { get; set; }
  }
  public class ActionConfig : DefaultBase
  {
    private string _fileNameTemplate = string.Empty;
    private string _dateFormat = string.Empty;
    private string _selectStatement = string.Empty;

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string FileNameTemplate
    {
      get => string.IsNullOrEmpty(_fileNameTemplate) ? _defaults?.FileNameTemplate ?? string.Empty : _fileNameTemplate;
      set => _fileNameTemplate = value;
    }
    public string DateFormat { 
      get => string.IsNullOrEmpty(_dateFormat) ? _defaults?.DateFormat ?? string.Empty : _dateFormat;
      set => _dateFormat = value; 
    }

    private List<TableConfig> _tables = new List<TableConfig>();
    public List<TableConfig> Tables { 
      get => _tables; 
      set 
      {
        _tables = value;
        foreach (var table in _tables)
        {
          table.ParentActionConfig = this;
        }
        _tables = value;
      } 
    }
    public string? SQLStatement { get; set; }
    public string SelectStatement
    {
      get => string.IsNullOrEmpty(_selectStatement) ? _defaults?.SelectStatement ?? string.Empty : _selectStatement;
      set => _selectStatement = value;
    }

  }

  public class DatabaseConfig : DefaultBase
  {
    private string _source = string.Empty;
    private string _target = string.Empty;
    public ActionConfig? ActionConfig { get; set; }
    public string? Name { get; set; }
    public string? DatabaseName { get; set; }

    public string SourcePathAndName { 
      get => this.Source + this.DatabaseName;
    }
    public string Source {
      get => string.IsNullOrEmpty(_source) ? _defaults?.Source ?? string.Empty : _source;
      set => _source = value;
    }

    public string TargetPathAndName
    {
      get
      { 
        return  this.Target + ProcessTemplate(this.ActionConfig?.FileNameTemplate ?? string.Empty, new
           {
             Name = this.Name,
             Date = DateTime.Now.ToString(this.ActionConfig?.DateFormat ?? "MM-dd-yyyy"),
           });
      }
            
    }
    public string Target 
    {
      get => string.IsNullOrEmpty(_target) ? _defaults?.Source ?? string.Empty : _target;
      set => _target = value;
    }

    private static string ProcessTemplate(string template, object data)
    {
      if (string.IsNullOrEmpty(template)) return string.Empty;

      var compiledTemplate = Handlebars.Compile(template);
      return compiledTemplate(data);
    }
  }

  public class TableConfig : DefaultBase
  {
    // Reference to parent ActionConfig
    private ActionConfig _parentActionConfig = new ActionConfig();
    private string _selectStatement = string.Empty;

    public ActionConfig ParentActionConfig
    {
      get => _parentActionConfig;
      set
      {
        _parentActionConfig = value;
      }
    }
    public string? Name { get; set; }
    public string? SQLCommand { get; set; }

    public List<Filters>? Filters { get; set; }
    public string SelectStatement
    {
      get => string.IsNullOrEmpty(_selectStatement) ? _parentActionConfig?.SelectStatement ?? string.Empty : _selectStatement;
      set => _selectStatement = value;
    }

  } 

    public class Filters
    {
      public string? Action { get; set; }
      public string? Operator { get; set; }
      public string? KeyField { get; set; }
      public string? KeyValue { get; set; }
    }
  public abstract class DefaultBase
  {
    protected DefaultsConfig? _defaults = null;

    public void InjectDefaults(DefaultsConfig defaults) => _defaults = defaults;
  }
}

