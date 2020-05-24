using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
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

        public override string ToString(){           
            return $@"   Scheduled shutdown data:\n
                           - GUID: {GUID.ToString()}\n
                           - Mode: {Mode.ToString()}\n
                           - Shutdown on: {Shutdown.ToString()}\n";
        }
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
        Task<Schedule> WatchChangesAsync(Action<Schedule> handler);
        Task<Schedule> CancelScheduleAsync(Guid guid);
    }

    class Worker : IUST1
    {        

        public static string Path { get { return _path.ToString(); } }        

        public static string Service { get{ return _path.ToString().Replace("/", ".").TrimStart('.'); } } 

        public ObjectPath ObjectPath { get { return _path; } }

        private static readonly ObjectPath _path = new ObjectPath("/net/xeill/elpuig/UST1"); 

        private static UST.Server Server;
        
        public Worker(UST.Server srv){
            Server = srv;
        }
            
        public Task<Schedule> RequestScheduleAsync()
        {            
            return Task.FromResult(Server.Current);
        }

        public Task<Schedule> WatchChangesAsync(Action<Schedule> handler)
        {
            Server.AddWatcher(handler);
            return Task.FromResult(Server.Current);
        }


        public Task<Schedule> CancelScheduleAsync(Guid guid)
        {
            if(Server.Current.GUID.Equals(guid)) return Task.FromResult(Server.Next());
            else return Task.FromResult(Server.Current);
        }
    }
}