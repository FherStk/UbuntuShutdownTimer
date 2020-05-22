/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses: 
      - gRPC for C# by gRPC.io:  under the Apache-2.0 License (https://github.com/grpc/grpc)
      - Protobuf by Google Inc.: under the Copyright  License (https://github.com/protocolbuffers/protobuf)

    This file is part of Ubuntu Shutdown Timer (UST from now on).

    UST is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    UST is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with UST.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
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
