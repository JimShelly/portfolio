using Microsoft.Extensions.Configuration;

namespace DbUtiller
{
  public static class Constants
  {
    public static class Logs
    {
      public const string NoDatabasesFoundMessage = "No databases found in appsettings.json";
      public const string NoScriptsFoundMessage = "No scripts found in appsettings.json"; 
      public const string ErrorMessage = "An error occurred";
      public const string DatabaseNameMissingMessage = "Database name is missing.";


    }

  }
}
