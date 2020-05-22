/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses: 
      - gRPC for C# by gRPC.io:  under the Apache-2.0 License (https://github.com/grpc/grpc)
      - Protobuf by Google Inc.: under the Copyright  License (https://github.com/protocolbuffers/protobuf)

    This file is part of Ubuntu Shutdown Timer (UST from now on).

    UST is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    UST is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with UST.  If not, see <https://www.gnu.org/licenses/>.
*/

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