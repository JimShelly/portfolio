// Importing necessary namespaces
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DbUtiller
{
    static class Program
    {
        // Main entry point for the application
        [STAThread]
        static async Task<int> Main(string[] args)
        {
            // Close Access if it is open
            Utils.CloseAccessIfOpen();
                // Configure Serilog
            // Pass the args off to the application to actually do the work.  
            Application application = new Application();
            await application.Run(args);


            Utils.CloseAccessIfOpen();
  

            return await Task.FromResult(0);
        }
    }
}