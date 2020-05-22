using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Logging;

namespace UST.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {   
            var host = CreateHostBuilder(args).ConfigureAppConfiguration((hostingContext, config) =>{                               
                config.SetBasePath(Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, "Settings"));                                
            }).Build();

            
            var logger = (ILogger<Program>)host.Services.GetService(typeof(ILogger<Program>));
            logger.LogInformation("Starting UST Server");

            Shutdown.Instance.Load(host);  
            Shutdown.Instance.Next();
            host.Run();                
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
