using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using UST.Server;

namespace UST.Client
{
    class Program
    {
        static void Main(string[] args)
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
            var s = client.GetSchedule(new GetScheduleRequest());
            Console.WriteLine("OK");
            ScheduleMessage(client, s);        
        }

        private static void ScheduleMessage(Service.ServiceClient client, Schedule s){
            Console.WriteLine("   Scheduled shutdown server data:");
            Console.WriteLine("   - GUID: {0}", s.Guid.ToString());
            Console.WriteLine("   - Mode: {0}", s.Mode.ToString());
            Console.WriteLine("   - Shutdown on: {0}", s.Shutdown.ToString());

            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);       
            //###### INIT DEVEL (REMOVE ON PRODUCTION) ######                          
            s.Shutdown = DateTime.SpecifyKind(now.AddSeconds(5), DateTimeKind.Utc).ToTimestamp();          
            //###### END  DEVEL (REMOVE ON PRODUCTION) ######      
            Console.Write("Schedulling the message box to rise on {0} with GUID {1}... ", s.Shutdown, s.Guid);                        
            var t = Task.Delay((int)(s.Shutdown - now.ToTimestamp()).Seconds*1000);
            Console.WriteLine("OK");            

            t.Wait();
            Cancel(client, s);
        }

        private static void Cancel(Service.ServiceClient client, Schedule s){
            Console.WriteLine("The user requests for cancellation over the scheduled shutdown on {0} with GUID {1}", s.Shutdown.ToString(), s.Guid); 
            Console.Write("Requesting for the current shutdown event cancellation... ");
            s = client.CancelCurrent(new CancelCurrentRequest(){ Guid = s.Guid});
            Console.WriteLine("OK");
            ScheduleMessage(client, s);
        }

        private static void Continue(Schedule s){
            Console.WriteLine("The user accepts the scheduled shutdown on {0} with GUID {1}", s.Shutdown.ToString(), s.Guid);  
        }
    }
}