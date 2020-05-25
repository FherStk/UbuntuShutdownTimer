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
using System.Collections.Concurrent;
using Tmds.DBus;
using UST1.DBus;

namespace UST
{   
    //Tested multi-server and multi-client; only the last registered server is processing the requests
    //All the registered clients are working ok, only 1 server can work ok.

    class Server
    {
        private class Settings{
            public Schedule[] Schedule {get; set;}
            public int PopupTimeframe {get; set;}
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
            var now = DateTime.Now;       
            var json = JsonSerializer.Deserialize<Settings>(File.ReadAllText(System.IO.Path.Combine("settings", "settings.json")));
            if(json.Schedule.Length == 0) throw new Exception("No data has been provided, please fill the setting.json file.");  

            _index = -1;
            _dbus = new Worker(this);
            _data = json.Schedule.OrderBy(x => x.Shutdown).ToList();  
            _data.ForEach((x) => {
                var dt = x.GetShutdownDateTime();
                x.GUID = Guid.NewGuid();
                x.PopupTimeframe = json.PopupTimeframe;    
                x.SetShutdownDateTime(new DateTime(now.Year, now.Month, now.Day, dt.Hour, dt.Minute, dt.Second));
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

                Console.WriteLine();
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
                Console.WriteLine(Current.ToString());
                Console.WriteLine();
                
                _cancel.Cancel();
            }

            //Get the next schedule
            var now = DateTime.Now;
            for(_index = _index+1; _index < _data.Count(); _index++){                
                if((Current.GetShutdownDateTime() - now).TotalMilliseconds > 0) break;
            }

            //If all the schedules has been used, start again for tomorrow
            if(Current == null){
                _index = 0;
                _data.ForEach(x => x.SetShutdownDateTime(x.GetShutdownDateTime().AddDays(1)));                                
            }
            
            //###### INIT DEVEL (REMOVE ON PRODUCTION) ######
            Current.SetShutdownDateTime(DateTime.Now.AddMinutes(2));
            Current.PopupTimeframe = 1;
            //###### END  DEVEL (REMOVE ON PRODUCTION) ######
            _cancel = new CancellationTokenSource();
            Task.Delay((int)(Current.GetShutdownDateTime() - now).TotalMilliseconds, _cancel.Token).ContinueWith(t =>
            {
                if(!t.IsCanceled){
                    Console.WriteLine("Shutting down the computer for the current scheduled event:");
                    Console.WriteLine(Current.ToString());
                    Console.WriteLine();
                    Console.WriteLine("SHUTDOWN!");  
                }
            });
            
            Console.WriteLine($"A new shutdown event has been successfully scheduled:");                
            Console.WriteLine(Current.ToString());

            return Current;
        }
    }
}