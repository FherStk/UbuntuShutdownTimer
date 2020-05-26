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
            
                Console.WriteLine("Requesting for the current shutdown event data:");
                var s = await _dbus.RequestScheduleAsync();            
                Console.WriteLine(s.ToString());
                Console.WriteLine();

                SchedulePopup(s);

                Console.WriteLine("Client ready and waiting!"); 
                Console.WriteLine();  

                while (true) { 
                    await Task.Delay(int.MaxValue);
                }     
            }    
        }     

        private void SchedulePopup(Schedule s){                    
            Console.WriteLine("Schedulling a new popup: ");
            Console.WriteLine(s.ToString());
            Console.WriteLine();          

            _cancel = new CancellationTokenSource();
            Task.Delay((int)(s.GetPopupDateTime() - DateTime.Now).TotalMilliseconds, _cancel.Token).ContinueWith(t =>
            {
                if(!t.IsCanceled){
                    Console.WriteLine("Rising the current scheduled popup: ");  
                    Console.WriteLine(s.ToString());  
                    Console.WriteLine();                          
                    
                    //Get user response (cancel or continue)
                    //Cancel(s);
                    //Continue(s);
                }
            });

            _dbus.WatchChangesAsync((sn) => {
                Console.WriteLine("The server cancelled the current scheduled popup:"); 
                Console.WriteLine(s.ToString());  
                Console.WriteLine();

                _cancel.Cancel();                 
                SchedulePopup(sn);
            });                                
        }

        private void Cancel(Schedule s){
            Console.WriteLine("The user requests for cancellation over the current scheduled shutdown:"); 
            Console.WriteLine(s.ToString());  
            Console.WriteLine();
           
            _dbus.CancelScheduleAsync(s.GUID);
        }

        private void Continue(Schedule s){
            Console.WriteLine("The user accepts the current scheduled shutdown:");
            Console.WriteLine(s.ToString());  
            Console.WriteLine();
        }
    }
}