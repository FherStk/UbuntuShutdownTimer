using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace UST.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).ConfigureAppConfiguration((hostingContext, config) =>
            {
                //TODO: set "Properties" or "Settings" as the folder where load "appsettings" to move them from root
                // var path = Path.Combine(Directory.GetCurrentDirectory(), "path/to/files");
                // config.AddKeyPerFile(directoryPath: path, optional: true);
            }).Build().Run();
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
