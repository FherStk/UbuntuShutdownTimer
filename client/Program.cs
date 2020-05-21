using System;
using System.Net.Http;
using System.Threading.Tasks;
using UST.Server;
using Grpc.Net.Client;

namespace UST.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            // The port number(5000) must match the port of the gRPC server.            
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");            
            var client = new Service.ServiceClient(channel);
            var reply = await client.SayHelloAsync(
                new HelloRequest { Name = "GreeterClient" }
            );

            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}