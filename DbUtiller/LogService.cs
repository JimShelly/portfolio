using Microsoft.Extensions.Logging;

namespace DbUtiller
{
  public class LogService
  {
    private readonly ILogger<LogService> _logger;

    public LogService(ILogger<LogService> logger)
    {
      _logger = logger;
    }

    public void Run()
    {
      // _logger.LogInformation("Running the service...");
      // _logger.LogWarning("This is a warning.");
      // _logger.LogError("This is an error.");
    }
  }
}