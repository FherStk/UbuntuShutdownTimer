using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Google.Protobuf.WellKnownTypes;

namespace UST.Server
{
    public sealed class Shutdown
    {
        private static readonly Shutdown _instance = new Shutdown();
        public List<Schedule> Data {get; private set;}
        
        static Shutdown(){
        }

        private Shutdown(){
        }

        public static Shutdown Instance{ 
            get{
                return _instance;
            }
        }

        public void Load(IConfiguration configuration){
            if(this.Data != null) throw new Exception("Data already loaded!");

            this.Data = new List<Schedule>();    
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            
            foreach(var item in configuration.GetSection("Schedule").GetChildren()){                    
                var dt = DateTimeOffset.Parse(item.GetSection("Shutdown").Get<string>());
                this.Data.Add(new Schedule(){                        
                    Guid = Guid.NewGuid().ToString(),
                    Mode = item.GetSection("Mode").Get<Schedule.Types.Mode>(),
                    Shutdown =  DateTime.SpecifyKind(new DateTime(now.Year, now.Month, now.Day, dt.Hour, dt.Minute, dt.Second), DateTimeKind.Utc).ToTimestamp(),                        
                });
            }
        }
    }
}