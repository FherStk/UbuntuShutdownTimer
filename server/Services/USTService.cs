using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace UST.Server
{
    public class USTService : Service.ServiceBase
    {
        private readonly ILogger<USTService> _logger;
        private readonly IConfiguration _configuration;
        
        public USTService(ILogger<USTService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;           
        }        

        public override Task<Schedule> GetSchedule(GetScheduleRequest request, ServerCallContext context)
        {     
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            var next = Shutdown.Instance.Data.Select(x => (Schedule: x, TimeLeft: (x.Shutdown - now.ToTimestamp()).Seconds)).Where(x => x.TimeLeft > 0).OrderBy(x => x.TimeLeft).Select(x => x.Schedule).FirstOrDefault();
            
            //TODO: if null, repeat for tomorrow dates            
            return Task.FromResult(next);
        }      
    }
}
