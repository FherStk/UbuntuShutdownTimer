using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace UST.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).ConfigureAppConfiguration((hostingContext, config) =>
            {               
                config.SetBasePath(Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, "Settings"));               
            }).Build().Run();

            //TODO: Read schedule config from settings
            //ConfigurationManager.AppSettings
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {                
                webBuilder.UseStartup<Startup>();
            }
        );
    }
}
