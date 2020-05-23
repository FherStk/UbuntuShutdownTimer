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
    //Tested multi-server and multi-client; only the last registered server is processing the requests
    //All the registered clients are working ok, only 1 server can work ok.

    class Server
    {
        private Worker _worker;

        public Server(){
            _worker = new Worker();
        }

        public async Task Run(){  
            Console.WriteLine("Running on server mode:");
            Console.Write("  Setting up connection...     ");
            using (var connection = new Connection(Address.System)){   
                var info = await connection.ConnectAsync();                
                Console.WriteLine("OK");

                Console.Write("  Setting up dbus service...   ");
                await connection.RegisterServiceAsync(UST1.DBus.Worker.Path);
                Console.WriteLine("OK");    

                Console.Write("  Setting up dbus interface... ");
                await connection.RegisterObjectAsync(_worker);
                Console.WriteLine("OK");

                Console.WriteLine();
                Console.WriteLine("Server ready and listening!");             
                
                while (true) { 
                    await Task.Delay(int.MaxValue);
                }
            }           
        }               
    }
}