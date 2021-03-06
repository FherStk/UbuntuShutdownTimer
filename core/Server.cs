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
using System.Linq;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tmds.DBus;
using UST1.DBus;

namespace UST
{   
    class Server
    {
        private class Settings{
            public Schedule[] Schedule {get; set;}
            public int PopupThreshold {get; set;}
            public int IgnoreThreshold {get; set;}
            public int AutocancelThreshold {get; set;}
        }

        private Worker _dbus;
        private List<Schedule> _data;          
        private CancellationTokenSource _cancel;
        private int _index;        
        public Schedule Current{
            get{
                if(_data == null || _data.Count == 0 || _index < 0 || _index >= _data.Count) return null;
                else return _data[_index];
            }
        }                 

        public Server(){
            var now = DateTimeOffset.Now;       
            var json = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Utils.GetFilePath("settings.json", "settings")));
            if(json.Schedule.Length == 0) throw new Exception("No data has been provided, please fill the setting.json file.");  

            _index = -1;
            _dbus = new Worker(this);
            _data = json.Schedule.OrderBy(x => x.Shutdown).ToList();  
            _data.ForEach((x) => {
                var dt = x.GetShutdownDateTime();
                x.GUID = Guid.NewGuid();
                x.PopupThreshold = json.PopupThreshold;    
                x.IgnoreThreshold = json.IgnoreThreshold;
                x.AutocancelThreshold = json.AutocancelThreshold;
                x.SetShutdownDateTime(new DateTimeOffset(now.Year, now.Month, now.Day, dt.Hour, dt.Minute, dt.Second, new TimeSpan()));
            });                  
        }

        public async Task Run(){  
            Console.WriteLine("Running on server mode:");
            Console.Write("  Setting up connection...     ");
            using (var connection = new Connection(Address.System)){   
                var info = await connection.ConnectAsync();                
                Console.WriteLine("OK");

                Console.Write("  Setting up dbus service...   ");
                await connection.RegisterServiceAsync(UST1.DBus.Worker.Service);
                Console.WriteLine("OK");    

                Console.Write("  Setting up dbus interface... ");
                await connection.RegisterObjectAsync(_dbus);
                Console.WriteLine("OK");
                Console.WriteLine();
                
                Next();

                Console.WriteLine("Server ready and listening!"); 
                Console.WriteLine();            
                                                
                while (true) { 
                    await Task.Delay(int.MaxValue);
                }
            }           
        }

        public Schedule Next(){
            //Cancel the current scheduled shutdown
            if(_cancel != null){                
                Console.WriteLine($"A client requests for cancellation over the current scheduled shutdown:");  
                Console.WriteLine(Current.ToString());  //Current wont be null when cancelling
                Console.WriteLine();
                
                _cancel.Cancel();
            }

            //Get the next schedule
            var now = DateTimeOffset.Now;
            for(_index = _index+1; _index < _data.Count(); _index++){                
                //###### INIT DEVEL (REMOVE ON PRODUCTION) ######
                //For ignore testing (becuase the shutdown event is too close)
                // Current.SetShutdownDateTime(DateTimeOffset.Now.AddMinutes(1));  
                // Current.PopupThreshold = 1;                                     
                // Current.Mode = ScheduleMode.INFORMATIVE;

                //For auto-cancel testing
                // Current.SetShutdownDateTime(DateTimeOffset.Now.AddMinutes(15));  
                // Current.PopupThreshold = 14;                                     
                // Current.Mode = ScheduleMode.INFORMATIVE;

                //For regular testing
                // Current.SetShutdownDateTime(DateTimeOffset.Now.AddMinutes(1));  
                // Current.PopupThreshold = 1;     
                // Current.IgnoreThreshold = 0;
                // Current.AutocancelThreshold = 0;                                                
                // Current.Mode = ScheduleMode.INFORMATIVE;                                
                //###### END  DEVEL (REMOVE ON PRODUCTION) ######
                
                if((Current.GetShutdownDateTime() - now).TotalMinutes > Current.IgnoreThreshold) break;
            }

            //If all the schedules has been used, start again for tomorrow
            if(Current == null){
                _index = 0;
                _data.ForEach(x => {
                    x.GUID = Guid.NewGuid();
                    x.SetShutdownDateTime(x.GetShutdownDateTime().AddDays(1));                    
                });                                
            }
            
            _cancel = new CancellationTokenSource();
            Task.Delay((int)(Current.GetShutdownDateTime() - now).TotalMilliseconds, _cancel.Token).ContinueWith(t =>
            {
                if(!t.IsCanceled){
                    Console.WriteLine("Shutting down the computer for the current scheduled event:");
                    Console.WriteLine(Current.ToString());
                    Console.WriteLine();
                    
                    //### PRODUCTION ###
                    Utils.RunShellCommand("poweroff", true);

                    //### TEST ###
                    //Console.WriteLine("SHUTDOWN!");  
                }
            });
            
            Console.WriteLine($"A new shutdown event has been successfully scheduled:");                
            Console.WriteLine(Current.ToString());
            Console.WriteLine();

            return Current;
        }
    }
}