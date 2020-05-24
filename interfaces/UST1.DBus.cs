using System;
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