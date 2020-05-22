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

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
