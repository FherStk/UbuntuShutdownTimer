using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace UST.Server
{
    public class USTService : Service.ServiceBase
    {
        private readonly ILogger<USTService> _logger;
        
        public USTService(ILogger<USTService> logger)
        {
            _logger = logger;

            //load the scheduled data from the appsettings.json file
        }        

        public override Task<Schedule> GetSchedule(GetScheduleRequest request, ServerCallContext context)
        {
            //TODO: send the nearest scheduled shutdown
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            return Task.FromResult(new Schedule(){
                Behaviour = Schedule.Types.Behaviour.Cancellable,
                Shutdown = now.ToTimestamp()
            });
        }      
    }
}
