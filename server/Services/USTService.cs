using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
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

            //load the scheduled data from the appsettings.json file
        }        

        public override Task<Schedule> GetSchedule(GetScheduleRequest request, ServerCallContext context)
        {            
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new JsonTimestampDeserializer());
            

            var schedule = new List<Schedule>();       
            IConfigurationSection myArraySection = _configuration.GetSection("Schedule");
            var itemArray = myArraySection.AsEnumerable();     

            var ch = _configuration.GetSection("Schedule").GetChildren();            
            var myArray = _configuration.GetSection("Schedule").Get<Schedule[]>((config) => {
                config.BindNonPublicProperties = true;
            });

            var first = myArray[0];
            
            var dt =_configuration.GetSection("Schedule:0:Shutdown").Get<DateTime>();     
            var ts = DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToTimestamp();
            Console.WriteLine("EN BLANCO: {0}", first.Shutdown == null);            

            // foreach(var data in _configuration.GetSection("Shutdown").AsEnumerable()){
            //     // schedule.Add(new Schedule(){
            //     //     //Mode = Enum.TryParse(data., out StatusEnum myStatus);
            //     // });
            //     // new Schedule(){
            //     //                 Mode = Schedule.Types.Mode.Cancellable,
            //     //                 Shutdown = now.ToTimestamp()
            // }

            
            //TODO: send the nearest scheduled shutdown
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            return Task.FromResult(new Schedule(){
                Mode = Schedule.Types.Mode.Cancellable,
                Shutdown = now.ToTimestamp()
            });
        }      
    }
}
