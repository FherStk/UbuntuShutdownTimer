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
            return Task.FromResult(Shutdown.Instance.Current);
        }

        public override Task<Schedule> CancelCurrent(CancelCurrentRequest request, ServerCallContext context)
        {     
            if(Shutdown.Instance.Current.Guid.Equals(request.Guid)) return Task.FromResult(Shutdown.Instance.Next());
            else return Task.FromResult(Shutdown.Instance.Current);
        }
    }
}
