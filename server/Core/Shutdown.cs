/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses: 
      - gRPC for C# by gRPC.io:  under the Apache-2.0 License (https://github.com/grpc/grpc)
      - Protobuf by Google Inc.: under the Copyright  License (https://github.com/protocolbuffers/protobuf)

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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Google.Protobuf.WellKnownTypes;

namespace UST.Server
{
    public sealed class Shutdown
    {
        private static readonly Shutdown _instance = new Shutdown();
        private int Index = -1;
        private CancellationTokenSource CancelTask = null;
        private ILogger Logger;        
        public List<Schedule> Data {get; private set;}
        public Schedule Current {
            get{
                if(Data == null || Data.Count == 0 || Index < 0 || Index >= Data.Count) return null;
                else return Data[Index];
            }
        }               
        
        static Shutdown(){
        }

        private Shutdown(){
        }

        public static Shutdown Instance{ 
            get{
                return _instance;
            }
        }

        public void Load(IHost host){
            if(Data != null) throw new Exception("Data already loaded!");
                        
            Data = new List<Schedule>();                
            Logger = (ILogger<Shutdown>)host.Services.GetService(typeof(ILogger<Shutdown>));
            var configuration = (IConfiguration)host.Services.GetService(typeof(IConfiguration));
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);            

            foreach(var item in configuration.GetSection("Schedule").GetChildren()){                    
                var dt = DateTimeOffset.Parse(item.GetSection("Shutdown").Get<string>());
                Data.Add(new Schedule(){                        
                    Guid = Guid.NewGuid().ToString(),
                    Mode = item.GetSection("Mode").Get<Schedule.Types.Mode>(),
                    Shutdown =  DateTime.SpecifyKind(new DateTime(now.Year, now.Month, now.Day, dt.Hour, dt.Minute, dt.Second), DateTimeKind.Utc).ToTimestamp(),                        
                });                
            }
            
            Data = Data.OrderBy(x => x.Shutdown).ToList();  

            if(Data.Count == 0) throw new Exception("No data has been provided!");                      
            Logger.LogInformation("Shutdown schedule data sucessfully loaded");       
        }

        public Schedule Next(){
            //Cancelling the current one
            if(CancelTask != null){
                CancelTask.Cancel();
                Logger.LogInformation($"Cancelling the scheduled shutdown event with GUID {Current.Guid}");  
            }  

            //Get the next schedule
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);                 
            for(Index = Index+1; Index < Data.Count(); Index++){                
                if((Current.Shutdown - now.ToTimestamp()).Seconds > 0) break;
            }

            //If null, schedule for tomorrow
            if(Current == null){
                Index = 0;
                Data.ForEach(x => x.Shutdown = x.Shutdown.ToDateTimeOffset().AddDays(1).ToTimestamp());                                
            }
            
            //###### INIT DEVEL (REMOVE ON PRODUCTION) ######
            //Current.Shutdown = DateTime.SpecifyKind(DateTime.Now.AddSeconds(30), DateTimeKind.Utc).ToTimestamp();    
            //###### END  DEVEL (REMOVE ON PRODUCTION) ######
            CancelTask = new CancellationTokenSource();
            Task.Delay((int)(Current.Shutdown - now.ToTimestamp()).Seconds*1000, CancelTask.Token).ContinueWith(t =>
            {
                if(!t.IsCanceled){
                    Logger.LogInformation($"Shutting down for scheduled event with GUID {Current.Guid}");  
                    Logger.LogInformation("SHUTDOWN!");  
                }
            });
            
            Logger.LogInformation($"A new shutdown event has been successfully scheduled on {Current.Shutdown} for GUID {Current.Guid}");    
            return Current;
        }
    }
}