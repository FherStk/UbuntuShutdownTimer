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
        private Task Task = null;
        private ILogger Logger;        
        public List<Schedule> Data {get; private set;}
        public Schedule Current {
            get{
                if(Data == null || Data.Count == 0 || Index < 0 || Index > Data.Count) return null;
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

        public void Next(){
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

            if(Task != null){
                Task.Dispose();  
                Logger.LogInformation("The current shutdown event has been cancelled.");
            }  
            
            Current.Shutdown = DateTime.SpecifyKind(DateTime.Now.AddSeconds(5), DateTimeKind.Utc).ToTimestamp();    //for develop
            Task = new Task(() => {
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                var wait = (Current.Shutdown - now.ToTimestamp()).Seconds;
                Thread.Sleep((int)wait*1000);

                Logger.LogInformation("Shutting down!");  
            });

            Task.Start();
            Logger.LogInformation($"A new shutdown event has been successfully scheduled on {Current.Shutdown}");    
        }
    }
}