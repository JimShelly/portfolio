using CommandLine;

namespace DbUtiller
{

  // Define options for the 'purge' verb
  [Verb("purge", HelpText = "Purge data from the database.")]
  public class PurgeOptions : GlobalOptions
  {
    [Option('d', "date", Required = false, HelpText = "Specify the date to purge records before. Format is 'YYYY-MM-DD'. Default is 365 days ago.")]
    public string? PurgeDate { get; set; }
  }

  // Define options for the 'prune' verb
  [Verb("prune", HelpText = "Prune unnecessary or old data from the database.")]
  public class PruneOptions : GlobalOptions
  {

  }

  [Verb("pack", HelpText = "Pack the database.")]
  public class PackOptions : GlobalOptions
  {

  }

  [Verb("propagate", HelpText = "Duplicate or Clone the database.")]
  public class PropagateOptions : GlobalOptions
  {
  }

  [Verb("preserve", HelpText = "Make a backup of the databases.")]
  public class PreserveOptions : GlobalOptions
  {

  }

  [Verb("program", HelpText = "Program options.")]
  public class ProgramOptions : GlobalOptions
  {
    [Option('s', "settings", Required = false, HelpText = "The action to perform.")]
    public bool settings { get; set; }

    [Option('r', "resources", Required = false, HelpText = "Displays the embedded resources.")]
    public bool resources { get; set; }
  }
  public class GlobalOptions
  {

    [Option('a', "auto", Required = false, HelpText = "Sets whether the program should prompt to close")]
    public bool Auto { get; set; }

    [Option('n', "name", Required = false, HelpText = "The name of the database.")]
    public string? Name { get; set; }

  }
}