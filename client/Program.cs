using System;
using System.Threading.Tasks;
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
            Console.WriteLine("Connecting to '{0}'... ", address);
            using var channel = GrpcChannel.ForAddress(address);            
            var client = new Service.ServiceClient(channel);            
            Console.WriteLine("OK");
            
            Console.WriteLine("Requesting for scheduled shutdown events... ");
            var reply = await client.GetScheduleAsync(new GetScheduleRequest());
            Console.WriteLine("OK");

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