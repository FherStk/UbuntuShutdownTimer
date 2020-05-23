using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
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
        Task<Schedule> CancelScheduleAsync(Guid guid);
    }

    class Worker : IUST1
    {
        private class Settings{
            public Schedule[] Schedule {get; set;}
        }

        public static string Path { get { return _path.ToString(); } }        

        public static string Service { get{ return _path.ToString().Replace("/", ".").TrimStart('.'); } } 

        public ObjectPath ObjectPath { get { return _path; } }

        private static readonly ObjectPath _path = new ObjectPath("/net/xeill/elpuig/UST1");

        private List<Schedule> _data;        
        
        private int _index;

        private CancellationTokenSource _cancel;
        
        private Schedule _current{
            get{
                if(_data == null || _data.Count == 0 || _index < 0 || _index >= _data.Count) return null;
                else return _data[_index];
            }
        }        
        
        public Worker(){
            var now = DateTime.Now;            
            var json = JsonSerializer.Deserialize<Settings>(File.ReadAllText(System.IO.Path.Combine("settings", "settings.json")));

            if(json.Schedule.Length == 0) throw new Exception("No data has been provided, please fill the setting.json file.");  
            _data = json.Schedule.OrderBy(x => x.Shutdown).ToList();  
            _data.ForEach(x => x.GUID = Guid.NewGuid());    

            Next();   
        }

        private Schedule Next(){
            //Cancel the current scheduled shutdown
            if(_cancel != null){
                _cancel.Cancel();
                Console.WriteLine($"Cancelling the scheduled shutdown event with GUID {_current.GUID}");  
            }

            //Get the next schedule
            var now = DateTime.Now;
            for(_index = _index+1; _index < _data.Count(); _index++){                
                if((_current.Shutdown - now).TotalMilliseconds > 0) break;
            }

            //If all the schedules has been used, start again for tomorrow
            if(_current == null){
                _index = 0;
                _data.ForEach(x => x.Shutdown = x.Shutdown.AddDays(1));                                
            }
            
            //###### INIT DEVEL (REMOVE ON PRODUCTION) ######
            //Current.Shutdown = DateTime.SpecifyKind(DateTime.Now.AddSeconds(30), DateTimeKind.Utc).ToTimestamp();    
            //###### END  DEVEL (REMOVE ON PRODUCTION) ######
            _cancel = new CancellationTokenSource();
            Task.Delay((int)(_current.Shutdown - now).TotalMilliseconds, _cancel.Token).ContinueWith(t =>
            {
                if(!t.IsCanceled){
                    Console.WriteLine("Shutting down for scheduled event with GUID {0}", _current.GUID);  
                    Console.WriteLine("SHUTDOWN!");  
                }
            });
            
            Console.WriteLine($"A new shutdown event has been successfully scheduled on {_current.Shutdown} for GUID {_current.GUID}");    
            return _current;
        }
    
        public Task<Schedule> RequestScheduleAsync()
        {            
            return Task.FromResult(_current);
        }

        public Task<Schedule> CancelScheduleAsync(Guid guid)
        {
            if(_current.GUID.Equals(guid)) return Task.FromResult(Next());
            else return Task.FromResult(_current);
        }

        
    }
}