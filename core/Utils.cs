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
using System.Diagnostics;

namespace UST
{ 
    public static class Utils{
        public static string AppFolder{
            get{
                return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            }
        } 
        
        public static string RunShellCommand(string cmd){
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var process = new Process()
            {                
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
        }
    }
}