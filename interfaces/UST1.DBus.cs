using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Linq;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace UST1.DBus
{    
    public enum ScheduleMode{
        CANCELLABLE,
        INFORMATIVE,
        SILENT
    }
    
    class Schedule{    
        public Guid GUID {get; set;}
        public DateTime Shutdown {get; set;}

        [JsonConverter(typeof(ScheduleModeConverter))]
        public ScheduleMode Mode {get; set;}
    }

    public class ScheduleModeConverter : JsonConverter<ScheduleMode>
    {
        public override ScheduleMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Enum.Parse<ScheduleMode>(reader.GetString());
        public override void Write(Utf8JsonWriter writer, ScheduleMode mode, JsonSerializerOptions options) => writer.WriteStringValue(mode.ToString());
    }

    [DBusInterface("net.xeill.elpuig.UST1")]
    interface IUST1 : IDBusObject
    {
        Task<Schedule> RequestScheduleAsync();
        Task<string> PingAsync();
    }

    class Worker : IUST1
    {
        public static readonly ObjectPath Path = new ObjectPath("/net/xeill/elpuig/UST1");
        public static string Service {
            get{
                return Path.ToString().Replace("/", ".").TrimStart('.');
            }
        } 

        private List<Schedule> Data;        
        
        private class Settings{
            public Schedule[] Schedule {get; set;}
        }
        
        public Worker(){
            var now = DateTime.Now;            
            var data = JsonSerializer.Deserialize<Settings>(File.ReadAllText(System.IO.Path.Combine("settings", "settings.json")));

            if(data.Schedule.Length == 0) throw new Exception("No data has been provided, please fill the setting.json file.");  
            this.Data = data.Schedule.OrderBy(x => x.Shutdown).ToList();  
            this.Data.ForEach(x => x.GUID = Guid.NewGuid());       
        }

    //     private Schedule Next(){
    //         //Cancelling the current one
    //         if(CancelTask != null){
    //             CancelTask.Cancel();
    //             Logger.LogInformation($"Cancelling the scheduled shutdown event with GUID {Current.Guid}");  
    //         }  

    //         //Get the next schedule
    //         var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);                 
    //         for(Index = Index+1; Index < Schedule.Count(); Index++){                
    //             if((Current.Shutdown - now.ToTimestamp()).Seconds > 0) break;
    //         }

    //         //If null, schedule for tomorrow
    //         if(Current == null){
    //             Index = 0;
    //             Schedule.ForEach(x => x.Shutdown = x.Shutdown.ToDateTimeOffset().AddDays(1).ToTimestamp());                                
    //         }
            
    //         //###### INIT DEVEL (REMOVE ON PRODUCTION) ######
    //         //Current.Shutdown = DateTime.SpecifyKind(DateTime.Now.AddSeconds(30), DateTimeKind.Utc).ToTimestamp();    
    //         //###### END  DEVEL (REMOVE ON PRODUCTION) ######
    //         CancelTask = new CancellationTokenSource();
    //         Task.Delay((int)(Current.Shutdown - now.ToTimestamp()).Seconds*1000, CancelTask.Token).ContinueWith(t =>
    //         {
    //             if(!t.IsCanceled){
    //                 Logger.LogInformation($"Shutting down for scheduled event with GUID {Current.Guid}");  
    //                 Logger.LogInformation("SHUTDOWN!");  
    //             }
    //         });
            
    //         Logger.LogInformation($"A new shutdown event has been successfully scheduled on {Current.Shutdown} for GUID {Current.Guid}");    
    //         return Current;
    //     }
    //  }
    

        public Task<Schedule> RequestScheduleAsync()
        {
            //Console.WriteLine("REQUEST!");

            return Task.FromResult(new Schedule());
        }

        public Task<string> PingAsync()
        {
            Console.WriteLine("REQUEST!");
            return Task.FromResult("pong");
        }

        public ObjectPath ObjectPath { get { return Path; } }
    }
}