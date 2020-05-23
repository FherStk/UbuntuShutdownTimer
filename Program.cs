/*
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
                Console.WriteLine("ERROR: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Details: ", ex.StackTrace);
            } 

            Console.WriteLine();
            

            // Console.WriteLine("Monitoring network state changes. Press Ctrl-C to stop.");

            // var systemConnection = Connection.System;
            // var networkManager = systemConnection.CreateProxy<IManager>("org.freedesktop.network1", "/org/freedesktop/network1");

            // // foreach (var device in await networkManager.)
            // // {
            // //     var interfaceName = await device.GetInterfaceAsync();
            // //     await device.WatchStateChangedAsync(
            // //         change => Console.WriteLine($"{interfaceName}: {change.oldState} -> {change.newState}")
            // //     );
            // // }

            // await Task.Delay(int.MaxValue);
        }


        private static async Task Server(){
            var server = new ServerConnectionOptions();
            using (var connection = new Connection(server))
            {
                await connection.RegisterObjectAsync(new UST1.DBus.Worker());   
                var boundAddress = await server.StartAsync("tcp:host=localhost");
                System.Console.WriteLine($"Server listening at {boundAddress}");     

                await Task.Delay(3600*1000).ContinueWith((t) => {
                    Console.WriteLine("Server closed");
                });        
            }
        }
        private static async Task Client(){
            var systemConnection = Connection.System;
            var dbusManager = systemConnection.CreateProxy<IUST1>("net.xeill.elpuig.UST1", "/net/xeill/elpuig/UST1");
            await dbusManager.AddContactAsync("name", "email").ContinueWith((reply) => {
                if(reply.Exception == null) Console.WriteLine("Reply: {0}", reply.Result);
                else Console.WriteLine("ERROR: {0}", reply.Exception.Message);
            });
        }
        private static async Task Config(){
            Console.WriteLine("Configuration requested: ", DateTime.Now.Year);

            var filename = "net.xeill.elpuig.UST1.service";
            var source = Path.Combine("files", filename);
            var dest = Path.Combine("/usr/share/dbus-1/services", filename);

            Console.Write("     Removing the current service... ");                
            File.Delete(dest);
            Console.WriteLine("OK");
            
            Console.Write("     Creating a new service... ");
            File.Copy(source, dest);
            Console.WriteLine("OK");

            filename = "net.xeill.elpuig.UST1.xml";
            source = Path.Combine("files", filename);
            dest = Path.Combine("/usr/share/dbus-1/interfaces", filename);

            Console.Write("     Removing the current interface... ");                
            File.Delete(dest);
            Console.WriteLine("OK");
            
            Console.Write("     Creating a new interface... ");
            File.Copy(source, dest);
            Console.WriteLine("OK");

            Console.Write("     Reloading DBus configuration... ");
            var systemConnection = Connection.System;
            var dbusManager = systemConnection.CreateProxy<IDBus>("org.freedesktop.DBus", "/org/freedesktop/DBus");
            await dbusManager.ReloadConfigAsync();
            Console.WriteLine("OK"); 
        }
        private static void Help(){
            Console.WriteLine("Usage: dotnet run [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --config        Configures the DBus interface (needs root permissions).");
            Console.WriteLine("  --client        Runs the application as a client (needs GUI user account).");
            Console.WriteLine("  --server        Runs the application as a server (needs a system account).");
        }
    }
}