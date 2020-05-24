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
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus;
using UST1.DBus;

namespace UST
{   
    //Tested multi-server and multi-client; only the last registered server is processing the requests
    //All the registered clients are working ok, only 1 server can work ok.

    class Client
    {
        private CancellationTokenSource _cancel;
        
        private IUST1 _dbus;
        
        public Client(){            
        }

        public async Task Run(){  
            Console.WriteLine("Running on client mode:");

            Console.Write("  Setting up connection...       ");
            using(var connection = Connection.System){
                Console.WriteLine("OK");
                
                Console.Write("  Conneting to dbus interface... ");
                _dbus = connection.CreateProxy<IUST1>(UST1.DBus.Worker.Service, UST1.DBus.Worker.Path);
                Console.WriteLine("OK");
                Console.WriteLine();
            
                Console.Write("  Requesting for the current shutdown event data:");
                var s = await _dbus.RequestScheduleAsync();            
                Console.WriteLine(s.ToString());

                SchedulePopup(s);
                while (true) { 
                    await Task.Delay(int.MaxValue);
                }     
            }    
        }     

        private void SchedulePopup(ISchedule s){
            var now = DateTime.Now;
            //###### INIT DEVEL (REMOVE ON PRODUCTION) ######                          
            s.Shutdown = now.AddSeconds(5);
            //###### END  DEVEL (REMOVE ON PRODUCTION) ######      
            Console.Write("  Schedulling the message box to rise on {0} with GUID {1}... ", s.Shutdown, s.GUID);

            _cancel = new CancellationTokenSource();
            Task.Delay((int)(s.Shutdown - now).TotalMilliseconds, _cancel.Token).ContinueWith(t =>
            {
                if(!t.IsCanceled){
                    Console.WriteLine("Rising popup for the scheduled event with GUID {0}", s.GUID);  
                    Console.WriteLine("POPUP!");  
                    
                    //Get user response (cancel or continue)
                    //Cancel(s);
                    //Continue(s);
                }
            });

            _dbus.WatchChangesAsync((sn) => {
                Console.Write("Cancelling the current scheduled event with GUID {0}... ", s.GUID); 
                _cancel.Cancel();     
                Console.WriteLine("OK");                                                     

                SchedulePopup(sn);
            });            

            Console.WriteLine("OK");                      
        }

        private void Cancel(ISchedule s){
            Console.WriteLine("  The user requests for cancellation over the scheduled shutdown on {0} with GUID {1}", s.Shutdown.ToString(), s.GUID); 
            
            Console.Write("  Requesting for the current shutdown event cancellation... ");            
            _dbus.CancelScheduleAsync(s.GUID);
            Console.WriteLine("OK");
        }

        private void Continue(ISchedule s){
            Console.WriteLine("  The user accepts the scheduled shutdown on {0} with GUID {1}", s.Shutdown.ToString(), s.GUID);  
        }
    }
}