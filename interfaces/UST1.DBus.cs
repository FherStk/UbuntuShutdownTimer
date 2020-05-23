using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace UST1.DBus
{
    [DBusInterface("net.xeill.elpuig.UST1")]
    interface IUST1 : IDBusObject
    {
        Task<uint> AddContactAsync(string Name, string Email);
        Task<string> PingAsync();
    }

    class Worker : IUST1
    {
        public static readonly ObjectPath Path = new ObjectPath("/net/xeill/elpuig/UST1");

        public Task<uint> AddContactAsync(string Name, string Email)
        {
            Console.WriteLine("REQUEST!");
            return Task.FromResult((uint)27);
        }

        public Task<string> PingAsync()
        {
            Console.WriteLine("REQUEST!");
            return Task.FromResult("pong");
        }

        public ObjectPath ObjectPath { get { return Path; } }
    }
}