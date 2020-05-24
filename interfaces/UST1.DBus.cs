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
        Task<ISchedule> RequestScheduleAsync();        
        Task<ISchedule> CancelScheduleAsync(Guid guid);
        Task<IDisposable> WatchChangesAsync(Action<ISchedule> handler);
    }

    [DBusInterface("net.xeill.elpuig.UST1.Schedule")]
    public interface ISchedule : IDBusObject, IDisposable
    {
        Guid GUID {get; set;}
        
        DateTime Shutdown {get; set;}
        
        ScheduleMode Mode {get; set;}
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
            
        public Task<ISchedule> RequestScheduleAsync()
        {            
            return Task.FromResult(Server.Current);
        }

        public Task<IDisposable> WatchChangesAsync(Action<ISchedule> handler)
        {
            Server.AddWatcher(handler);
            //return Task.FromResult(Server.Current);
            //return Task.FromResult<IDisposable>(Server.Current);
            return Task.FromResult<IDisposable>(new Connection(""));
        }


        public Task<ISchedule> CancelScheduleAsync(Guid guid)
        {
            if(Server.Current.GUID.Equals(guid)) return Task.FromResult(Server.Next());
            else return Task.FromResult(Server.Current);
        }
    }
}