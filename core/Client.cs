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
                                    
                    _cancel.Cancel();   
                    _current = sn;             
                    SchedulePopup();  
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
            var minutes = Math.Max(0, (int)(_current.GetPopupDateTime() - DateTimeOffset.Now).TotalMinutes);

            if(minutes < _current.AutocancelThreshold) Cancel(true);
            else{
                Task.Delay(minutes*60000, _cancel.Token).ContinueWith(t =>
                {
                    if(!t.IsCanceled){
                        Console.WriteLine("Rising the current scheduled popup: ");  
                        Console.WriteLine(_current.ToString());  
                        Console.WriteLine();                                              
                        Question();
                    }
                });        
            }
        }

        private void Question(){
            var title = "Aturada automàtica de l'equip";
            var message = $"Aquest equip te programada una aturada automàtica a les <b>{_current.GetShutdownDateTime().TimeOfDay.ToString()}</b>.\n\nSi su plau, desi els treballs en curs i tanqui totes les aplicacions";
            var timeout = _current.PopupThreshold*60;                    
            var cancel = string.Empty;

            switch(_current.Mode){                       
                case ScheduleMode.INFORMATIVE:                           
                    message += ".";
                    cancel += "--no-cancel";     
                    break;

                case ScheduleMode.CANCELLABLE:
                    message += " o premi 'cancel·lar' per anul·lar l'aturada automàtca de l'equip.";                                                  
                    break;
            }

            if(_current.Mode == ScheduleMode.SILENT) Silent();
            else
            {                       
                var result = Utils.RunShellCommand($"{Utils.GetFilePath("notify.sh")} {timeout} \"{title}\" \"{message}\" {cancel}");                
                if(result.StartsWith("shutdow")) Continue();
                else Cancel();
            }
        }

        private void Cancel(bool auto = false){
            Console.WriteLine($"{(auto ? "Auto-cancellation request" : "The user requests for cancellation" )} over the current scheduled shutdown:"); 
            Console.WriteLine(_current.ToString());  
            Console.WriteLine();
           
            _dbus.CancelScheduleAsync(_current.GUID);
            //if(!auto) Utils.RunShellCommand("zenity --notification --text=\"Heu cancel·lat l'aturada automàtica de l'equip, si us plau, \n<b>recordeu aturar-la manualment</b> quan acabeu de fer-la servir.\"");  //unable to set timeout
            if(!auto) Utils.RunShellCommand("notify-send -u critical -t 0 \"Atenció:\" \"Heu cancel·lat l'aturada automàtica de l'equip, si us plau, <b>recordeu aturar-lo manualment</b> quan acabeu de fer-lo servir.\"");
        }

        private void Continue(){
            Console.WriteLine("The user accepts the current scheduled shutdown:");
            Console.WriteLine(_current.ToString());  
            Console.WriteLine();

            //Utils.RunShellCommand("zenity --notification --text=\"\nL'equip <b>s'aturarà</b> automàticament en breus moments...\""); //unable to set timeout
            Utils.RunShellCommand("notify-send -u critical -t 0 \"\" \"L'equip <b>s'aturarà</b> automàticament en breus moments...\"");
        }

        private void Silent(){
            Console.WriteLine("The current scheduled shutdown is runing on silent mode:"); 
            Console.WriteLine(_current.ToString());  
            Console.WriteLine();
           
            _dbus.CancelScheduleAsync(_current.GUID);
        }
    }
}