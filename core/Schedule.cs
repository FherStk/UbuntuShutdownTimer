 /*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses: 
      - Tmds.DBus by Tom Deseyn: under the MIT License (https://www.nuget.org/packages/Tmds.DBus/)

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
using System.Text.Json;
using System.Text.Json.Serialization;
using Tmds.DBus;

namespace UST
{   
    public enum ScheduleMode{
        CANCELLABLE,
        INFORMATIVE,
        SILENT
    }

    [Dictionary]
    class Schedule: IDisposable
    {
        public Guid GUID {get; set;}
        public string Shutdown {get; set;}  //cannot be a datetime due serialization through d-bus

        [JsonConverter(typeof(ScheduleModeConverter))]
        public ScheduleMode Mode {get; set;}       

        public override string ToString(){           
            return $"  - GUID: {GUID.ToString()} \n  - Mode: {Mode.ToString()}\n  - Shutdown on: {Shutdown.ToString()}";
        }

        public void SetShutdownDateTime(DateTime dt){
            Shutdown = dt.ToString();
        }

        public DateTime GetShutdownDateTime(){
            return DateTime.Parse(Shutdown);
        }

        public void Dispose(){

        }
    }
    
    // public class Schedule : UST1.DBus.ISchedule{    
    //     public Guid GUID {get; set;}
    //     public DateTime Shutdown {get; set;}

    //     [JsonConverter(typeof(ScheduleModeConverter))]
    //     public ScheduleMode Mode {get; set;}

    //     public ObjectPath ObjectPath { get { return _path; } }

    //     private static readonly ObjectPath _path = new ObjectPath("/net/xeill/elpuig/UST1/Schedule"); 

    //     public void Dispose(){

    //     }

    //     public override string ToString(){           
    //         return $"  - GUID: {GUID.ToString()} \n  - Mode: {Mode.ToString()}\n  - Shutdown on: {Shutdown.ToString()}";
    //     }
    // }

    public class ScheduleModeConverter : JsonConverter<ScheduleMode>
    {        
        public override ScheduleMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Enum.Parse<ScheduleMode>(reader.GetString());
        public override void Write(Utf8JsonWriter writer, ScheduleMode mode, JsonSerializerOptions options) => writer.WriteStringValue(mode.ToString());
    }
}