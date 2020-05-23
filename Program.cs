﻿/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses: 
      - Tmds.DBus by Tom Deseyn: under the MIT License (https://www.nuget.org/packages/Tmds.DBus/)

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

using System;
using Tmds.DBus;
using UST1.DBus;
using DBus.DBus;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace UST
{   
    //Tested multi-server and multi-client; only the last registered server is processing the requests
    //All the registered clients are working ok, only 1 server can work ok.

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine();
            Console.Write("Ubuntu Shutdown Timer: ");
            Console.WriteLine("v1.0.0.0 (alpha-1)");
            Console.Write("Copyright © {0}: ", DateTime.Now.Year);
            Console.WriteLine("Fernando Porrino Serrano.");
            Console.Write("Under the AGPL license: ", DateTime.Now.Year);
            Console.WriteLine("https://github.com/FherStk/UbuntuShutdownTimer/blob/master/LICENSE");
            Console.WriteLine();

            try{
                if(args.Where(x => x.Equals("--config")).Count() == 1) await Config();                          
                else if(args.Where(x => x.Equals("--server")).Count() == 1) await Server();        
                else if(args.Where(x => x.Equals("--client")).Count() == 1) await Client();        
                else Help();
            }
            catch(Exception ex){
                Console.WriteLine("ERROR: {0}", ex.Message);
                Console.WriteLine("Details: {0}", ex.StackTrace);
            } 

            Console.WriteLine();
        }

        private static async Task Server(){  
            var t = new UST1.DBus.Worker();
            Console.WriteLine("Running on server mode:");
            Console.Write("  Setting up connection...     ");
            using (var connection = new Connection(Address.System)){   
                var info = await connection.ConnectAsync();                
                Console.WriteLine("OK");

                Console.Write("  Setting up dbus service...   ");
                await connection.RegisterServiceAsync(UST1.DBus.Worker.Path.ToString());
                Console.WriteLine("OK");    

                Console.Write("  Setting up dbus interface... ");
                await connection.RegisterObjectAsync(new UST1.DBus.Worker());
                Console.WriteLine("OK");

                Console.WriteLine();
                Console.WriteLine("Server ready and listening!");             
                
                while (true) { 
                    await Task.Delay(int.MaxValue);
                }
            }           
        }
        private static async Task Client(){
            Console.WriteLine("Running on client mode:");

            Console.Write("  Setting up connection...       ");
            var connection = Connection.System;
            Console.WriteLine("OK");
            
            Console.Write("  Conneting to dbus interface... ");
            var ust = connection.CreateProxy<IUST1>(UST1.DBus.Worker.Path.ToString(), UST1.DBus.Worker.Service);
            Console.WriteLine("OK");
            Console.WriteLine();

            await ust.PingAsync().ContinueWith((reply) => {
                if(reply.Exception == null) Console.WriteLine("Reply: {0}", reply.Result);
                else Console.WriteLine("ERROR: {0}", reply.Exception.Message);
            });

            while (true) { 
                await Task.Delay(int.MaxValue);
            }
        }
        
        private static async Task Config(){
            Console.WriteLine("Configuration requested: ", DateTime.Now.Year);
            var filename = "system-local.conf";
            var source = Path.Combine("files", filename);
            var dest = Path.Combine("/etc/dbus-1", filename);

            Console.Write("  Removing the current config file... ");                
            File.Delete(dest);
            Console.WriteLine("OK");
            
            Console.Write("  Creating a new config file...       ");
            File.Copy(source, dest);
            Console.WriteLine("OK");

            Console.Write("  Reloading dbus configuration...     ");
            var systemConnection = Connection.System;
            var dbusManager = systemConnection.CreateProxy<IDBus>("org.freedesktop.DBus", "/org/freedesktop/DBus");
            await dbusManager.ReloadConfigAsync();
            Console.WriteLine("OK"); 
        }

        private static void Help(){
            Console.WriteLine("Usage: dotnet run [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --config        Configures the DBus interface (needs root permissions)."); //TODO: --install (creates the service, edit the files (do not ovewrite)) and also --uninstall
            Console.WriteLine("  --client        Runs the application as a client (needs GUI user account).");
            Console.WriteLine("  --server        Runs the application as a server (needs a system account).");
        }
    }
}