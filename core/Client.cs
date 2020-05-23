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

    class Client
    {
        public Client(){            
        }

        public async Task Run(){  
            Console.WriteLine("Running on client mode:");

            Console.Write("  Setting up connection...       ");
            using(var connection = Connection.System){
                Console.WriteLine("OK");
                
                Console.Write("  Conneting to dbus interface... ");
                var ust = connection.CreateProxy<IUST1>(UST1.DBus.Worker.Path, UST1.DBus.Worker.Service);
                Console.WriteLine("OK");
                Console.WriteLine();

                Console.Write("  Requesting for the current shutdown event data... ");
                var s = await ust.RequestScheduleAsync();
                Console.WriteLine("OK");

                await ScheduleMessage(ust, s);                
                while (true) { 
                    await Task.Delay(int.MaxValue);
                }     
            }    
        }        

        private async static Task ScheduleMessage(IUST1 ust, Schedule s){
            Console.WriteLine(s.ToString());

            var now = DateTime.Now;
            //###### INIT DEVEL (REMOVE ON PRODUCTION) ######                          
            s.Shutdown = now.AddSeconds(5);
            //###### END  DEVEL (REMOVE ON PRODUCTION) ######      
            Console.Write("  Schedulling the message box to rise on {0} with GUID {1}... ", s.Shutdown, s.GUID);
            await Task.Delay((int)(s.Shutdown - now).TotalMilliseconds);
            Console.WriteLine("OK");            

            //Cancel(s);
        }

        private async static Task Cancel(IUST1 ust, Schedule s){
            Console.WriteLine("  The user requests for cancellation over the scheduled shutdown on {0} with GUID {1}", s.Shutdown.ToString(), s.GUID); 
            Console.Write("  Requesting for the current shutdown event cancellation... ");            
            s = await ust.CancelScheduleAsync(s.GUID);
            Console.WriteLine("OK");

            await ScheduleMessage(ust, s);
        }

        private static void Continue(Schedule s){
            Console.WriteLine("  The user accepts the scheduled shutdown on {0} with GUID {1}", s.Shutdown.ToString(), s.GUID);  
        }
    }
}