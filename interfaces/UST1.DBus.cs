using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Tmds.DBus;
using UST;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace UST1.DBus
{        
    [DBusInterface("net.xeill.elpuig.UST1")]
    interface IUST1 : IDBusObject
    {        
        Task<Schedule> RequestScheduleAsync();        
        Task CancelScheduleAsync(Guid guid);
        Task<IDisposable> WatchChangesAsync(Action<Schedule> handler);
    }

    class Worker : IUST1
    {        

        public event Action<Schedule> OnCancel;

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

        public Task<IDisposable> WatchChangesAsync(Action<Schedule> reply)
        {            
            return SignalWatcher.AddAsync(this, nameof(OnCancel), reply);            
        }

        public Task CancelScheduleAsync(Guid guid)
        {
            if(!Server.Current.GUID.Equals(guid)) return Task.FromResult(Server.Current);
            else {
                var s = Server.Next();
                OnCancel?.Invoke(s);                
                
                return new Task(() => {});
            }
        }
    }
}