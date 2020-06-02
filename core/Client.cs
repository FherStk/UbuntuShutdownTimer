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
        
        private IUST1 _dbus;

        private int _pid;
        private bool _ignore;
        
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

                Console.WriteLine("Requesting for the current shutdown event data:");
                var current = await _dbus.RequestScheduleAsync();            
                Console.WriteLine(current.ToString());
                Console.WriteLine();               

                Console.Write("  Subscribing to dbus notifications... ");
                await _dbus.WatchChangesAsync((next) => {
                    if(_cancel != null) _cancel.Cancel();                         
                    ServerCancel(current);                                        
                    SchedulePopup(next);  
                });
                Console.WriteLine("OK");  
                Console.WriteLine();
                
                SchedulePopup(current);

                Console.WriteLine("Client ready and waiting!"); 
                Console.WriteLine();  

                while (true) { 
                    await Task.Delay(int.MaxValue);
                }     
            }    
        }     

        private void SchedulePopup(Schedule current){                    
            Console.WriteLine("Schedulling a new popup: ");
            Console.WriteLine(current.ToString());
            Console.WriteLine();          

            _cancel = new CancellationTokenSource();
            var minutes = Math.Max(0, (int)(current.GetShutdownDateTime() - DateTimeOffset.Now).TotalMinutes);
            if(minutes < current.AutocancelThreshold) UserCancel(current, true);
            else{
                minutes = Math.Max(0, (int)(current.GetPopupDateTime() - DateTimeOffset.Now).TotalMinutes);
                Task.Delay(minutes*60000, _cancel.Token).ContinueWith(t =>
                {
                    if(!t.IsCanceled){
                        Console.WriteLine("Rising the current scheduled popup: ");  
                        Console.WriteLine(current.ToString());  
                        Console.WriteLine();                                              
                        Question(current);
                    }
                });        
            }
        }

        private void Question(Schedule current){
            var title = "Aturada automàtica de l'equip";
            var message = $"Aquest equip te programada una aturada automàtica a les <b>{current.GetShutdownDateTime().TimeOfDay.ToString()}</b>.\n\nSi su plau, desi els treballs en curs i tanqui totes les aplicacions";
            var timeout = (int)(current.GetShutdownDateTime() - DateTimeOffset.Now).TotalSeconds;

            switch(current.Mode){                       
                case ScheduleMode.INFORMATIVE:                                               
                    _pid = Utils.RunShellCommand($"{Utils.GetFilePath("notify.sh")} {timeout} \"{title}\" \"{message}.\" --no-cancel", new Action<string>((result) => {                        
                        UserAccept(current);
                    }));
                    break;

                case ScheduleMode.CANCELLABLE:   
                    _pid = Utils.RunShellCommand($"{Utils.GetFilePath("notify.sh")} {timeout} \"{title}\" \"{message} o premi 'cancel·lar' per anul·lar l'aturada automàtca de l'equip.\"", new Action<string>((result) => {
                        if(result.StartsWith("ACCEPT")) UserAccept(current);
                        else UserCancel(current);        
                    }));                    
                    break;

                case ScheduleMode.SILENT:
                    UserSilent(current);
                    break;
            }
        }

        private void UserCancel(Schedule current, bool auto = false){
            _pid = 0;
            _ignore = true;

            Console.WriteLine($"{(auto ? "Auto-cancellation request" : "The user requests for cancellation" )} over the current scheduled shutdown:"); 
            Console.WriteLine(current.ToString());  
            Console.WriteLine();
           
            _dbus.CancelScheduleAsync(current.GUID);
            if(!auto) Utils.RunShellCommand("notify-send -u critical -t 0 \"Atenció:\" \"Heu cancel·lat l'aturada automàtica de l'equip, si us plau, <b>recordeu aturar-lo manualment</b> quan acabeu de fer-lo servir.\"");
        }        

        private void UserAccept(Schedule current){
            _pid = 0;

            if(_ignore) _ignore = false;
            else{
                Console.WriteLine("The user accepts the current scheduled shutdown:");
                Console.WriteLine(current.ToString());  
                Console.WriteLine();

                Utils.RunShellCommand("notify-send -u critical -t 0 \"Atenció:\" \"L'equip <b>s'aturarà</b> automàticament en breus moments...\"");
            }
        }

        private void UserSilent(Schedule current){
            Console.WriteLine("The current scheduled shutdown is runing on silent mode:"); 
            Console.WriteLine(current.ToString());  
            Console.WriteLine();
        }

        private void ServerCancel(Schedule current){  
            if(_ignore) _ignore = false;
            else{                      
                Console.WriteLine("The server cancelled the current scheduled popup:"); 
                Console.WriteLine(current.ToString());  
                Console.WriteLine();

                if(_pid > 0){
                    _ignore = true;                    

                    try{
                        //Find our zenity instance (not 100% reliable)
                        _pid = int.Parse(Utils.RunShellCommand($"ps -e | awk '$4 == \"zenity\" && $1 >= \"{_pid}\"' | head -n 1 | awk '{{print $1}}'"));
                        Utils.RunShellCommand($"kill -9 {_pid}");
                    }
                    catch{
                        Utils.RunShellCommand($"pkill -9 zenity");
                    }
                    finally{
                        _pid = 0;    
                    }
                } 
                
                Utils.RunShellCommand("notify-send -u critical -t 0 \"Atenció:\" \"Un altre usuari ha cancel·lat l'aturada automàtica de l'equip, si us plau, <b>recordeu aturar-lo manualment</b> quan acabeu de fer-lo servir.\"");
            }
        }
    }
}