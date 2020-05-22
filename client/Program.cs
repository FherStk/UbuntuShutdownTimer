using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using UST.Server;

namespace UST.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Starting UST Client... ");
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            Console.WriteLine("OK");

            var address = "http://localhost:5000";
            Console.Write("Connecting to '{0}'... ", address);
            using var channel = GrpcChannel.ForAddress(address);            
            var client = new Service.ServiceClient(channel);            
            Console.WriteLine("OK");
            
            Console.Write("Requesting for the next shutdown event... ");
            var reply = await client.GetScheduleAsync(new GetScheduleRequest());
            Console.WriteLine("OK:");
            Console.WriteLine("   - GUID: {0}", reply.Guid.ToString());
            Console.WriteLine("   - Mode: {0}", reply.Mode.ToString());
            Console.WriteLine("   - Shutdown on: {0}", reply.Shutdown.ToString());
                                    
            var now = DateTime.SpecifyKind(DateTime.Now.AddSeconds(-1), DateTimeKind.Utc);                
            Console.Write("Schedulling the message box to rise on {0}... ", now.ToString());         
            var t = Task.Delay((int)(reply.Shutdown - now.ToTimestamp()).Seconds*1000).ContinueWith(t =>
            {                
                Console.Write("BESSAGEBOX");
            });
            Console.WriteLine("OK:");            
            await t;

            // Console.Write("Requesting for the current shutdown event cancellation... ");
            // var reply2 = await client.CancelCurrentAsync(new CancelCurrentRequest(){ Guid = reply.Guid});
            // Console.WriteLine("OK:");
            // Console.WriteLine("   - GUID: {0}", reply2.Guid.ToString());
            // Console.WriteLine("   - Mode: {0}", reply2.Mode.ToString());
            // Console.WriteLine("   - Shutdown on: {0}", reply2.Shutdown.ToString());

            //TODO: 
            //      1. Schedule messages for the schedule received
            //      2. When fired, request for cancellations
            //          2.1. If still scheduled and not silent, display message (zenity)
            //          2.2. Otherwise ignore
            //      3. The user chooses an option
            //          3.1. Abort: send abort request to the server
            //          3.2. Otherwise ignore
            //      4. GOTO 1            
        }
    }
}