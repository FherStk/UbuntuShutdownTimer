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
        private static ConcurrentBag<Schedule> _schedule;
        
        public USTService(ILogger<USTService> logger, IConfiguration configuration)
        {
            //TODO: this cannot be here, bust be loaded on program startup (before accepting any requests)
            _logger = logger;
            _configuration = configuration;

            if(_schedule == null){
                //NOTE: Get<Schedule[]> does not parse the Timestamp... After a lot of hours, I'm not able to register a cursom JSON parser for Timestamp...
                //var schedule = _configuration.GetSection("Schedule").Get<Schedule[]>(); 
                _schedule = new ConcurrentBag<Schedule>();    
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                
                foreach(var item in _configuration.GetSection("Schedule").GetChildren()){                    
                    var dt = DateTimeOffset.Parse(item.GetSection("Shutdown").Get<string>());
                    _schedule.Add(new Schedule(){                        
                        Guid = Guid.NewGuid().ToString(),
                        Mode = item.GetSection("Mode").Get<Schedule.Types.Mode>(),
                        Shutdown =  DateTime.SpecifyKind(new DateTime(now.Year, now.Month, now.Day, dt.Hour, dt.Minute, dt.Second), DateTimeKind.Utc).ToTimestamp(),                        
                    });
                }
            } 
        }        

        public override Task<Schedule> GetSchedule(GetScheduleRequest request, ServerCallContext context)
        {     
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            var next = _schedule.Select(x => (Schedule: x, TimeLeft: (x.Shutdown - now.ToTimestamp()).Seconds)).Where(x => x.TimeLeft > 0).OrderBy(x => x.TimeLeft).Select(x => x.Schedule).FirstOrDefault();
            
            //TODO: if null, repeat for tomorrow dates            
            return Task.FromResult(next);
        }      
    }
}
