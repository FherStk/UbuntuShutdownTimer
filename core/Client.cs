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
using System.IO;
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

        private Schedule _current;
        
        private IUST1 _dbus;
        
        public Client(){            
        }

        public async Task Run(){  
            Console.WriteLine("Running on client mode:");

            Console.Write("  Setting up connection...             ");
            using(var connection = Connection.System){
                Console.WriteLine("OK");
                
                Console.Write("  Conneting to dbus interface...       ");
                _dbus = connection.CreateProxy<IUST1>(UST1.DBus.Worker.Service, UST1.DBus.Worker.Path);
                Console.WriteLine("OK");                

                Console.Write("  Subscribing to dbus notifications... ");
                await _dbus.WatchChangesAsync((sn) => {
                    Console.WriteLine("The server cancelled the current scheduled popup:"); 
                    Console.WriteLine(_current.ToString());  
                    Console.WriteLine();
                                    
                    _cancel.Cancel();   //cancel the popup event 
                    _current = sn;             
                    SchedulePopup();  //schedule the new event                                
                });
                Console.WriteLine("OK");  
                Console.WriteLine();
            
                Console.WriteLine("Requesting for the current shutdown event data:");
                _current = await _dbus.RequestScheduleAsync();            
                Console.WriteLine(_current.ToString());
                Console.WriteLine();
                
                SchedulePopup();

                Console.WriteLine("Client ready and waiting!"); 
                Console.WriteLine();  

                while (true) { 
                    await Task.Delay(int.MaxValue);
                }     
            }    
        }     

        private void SchedulePopup(){                    
            Console.WriteLine("Schedulling a new popup: ");
            Console.WriteLine(_current.ToString());
            Console.WriteLine();          

            _cancel = new CancellationTokenSource();
            Task.Delay((int)(_current.GetPopupDateTime() - DateTime.Now).TotalMilliseconds, _cancel.Token).ContinueWith(t =>
            {
                if(!t.IsCanceled){
                    Console.WriteLine("Rising the current scheduled popup: ");  
                    Console.WriteLine(_current.ToString());  
                    Console.WriteLine();                          
                    
                    var title = "Aturada automàtica de l'equip";
                    var message = $"Aquest equip te programada una aturada automàtica a les <b>{_current.GetShutdownDateTime().TimeOfDay.ToString()}</b>.\nSi su plau, desi els treballs en curs i tanqui totes les aplicacions";
                    var zenity = $"zenity --progress --title=\"{title}\" --text=\"{message}\" --percentage=0 --auto-close --auto-kill --time-remaining";                    

                    //Get user response (cancel or continue)
                    //Cancel();
                    switch(_current.Mode){                       
                        case ScheduleMode.INFORMATIVE:
                            //zenity --notification --text="Hola" --window-icon="info"
                            //lo mismo que cancellable pero con --no-cancel
                            message += ".";
                            break;

                        case ScheduleMode.CANCELLABLE:
                            message += " o premi 'cancel·lar per anul·lar l'aturada automàtca de l'equip.";
                            zenity += "--no-cancel";                           
                            break;
                    }

                    if(_current.Mode != ScheduleMode.SILENT){
                        var script = $@"
                            #!/bin/bash
                            i=0
                            p=0

                            while [ $i -lt {_current.PopupTimeframe*60} ]
                            do
                                    i=$[$i + 1]
                                    echo $((10 * i))
                                    sleep 1
                                    p=$[$p + 1]
                            done > >({zenity})
                            echo 'shutdown'
                        ";

                        //Unable to read output (broken pipe...)
                        //var result = Utils.RunShellCommand(script);
                        var result = Utils.RunShellCommand(Path.Combine(Utils.AppFolder, "files", "notify.sh"));
                        Utils.RunShellCommand($"echo {result}");
                        //var tmp = 0;
                    }

                    //Continue();
                }
            });        
        }

        private void Cancel(){
            Console.WriteLine("The user requests for cancellation over the current scheduled shutdown:"); 
            Console.WriteLine(_current.ToString());  
            Console.WriteLine();
           
            _dbus.CancelScheduleAsync(_current.GUID);
        }

        private void Continue(){
            Console.WriteLine("The user accepts the current scheduled shutdown:");
            Console.WriteLine(_current.ToString());  
            Console.WriteLine();
        }
    }
}