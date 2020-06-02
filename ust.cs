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
using System.Linq;
using System.Threading.Tasks;

namespace UST
{   
    //Publish: dotnet publish -r linux-x64 -c Release
    class ust
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine();
            Console.Write("Ubuntu Shutdown Timer: ");
            Console.WriteLine("v1.0.0.0 (alpha-1)");
            Console.Write("Copyright © {0}: ", DateTime.Now.Year);
            Console.WriteLine("Fernando Porrino Serrano.");
            Console.Write("Under the AGPL license: ", DateTime.Now.Year);
            Console.WriteLine("https://github.com/FherStk/UbuntuShutdownTimer/blob/master/LICENSE");
            Console.WriteLine();

            try{
                if(args.Where(x => x.Equals("--install")).Count() == 1) new Installer().Install();                          
                else if(args.Where(x => x.Equals("--uninstall")).Count() == 1) new Installer().Uninstall();    
                else if(args.Where(x => x.Equals("--server")).Count() == 1) await new Server().Run();
                else if(args.Where(x => x.Equals("--client")).Count() == 1) await new Client().Run();
                else Help();
            }
            catch(Exception ex){
                Console.WriteLine("ERROR: {0}", ex.Message);
                Console.WriteLine("Details: {0}", ex.StackTrace);
            } 

            Console.WriteLine();
        }

        private static void Help(){
            Console.WriteLine("Usage: dotnet run [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");            
            Console.WriteLine("  --server        Runs the application as a server (needs a system account).");
            Console.WriteLine("  --client        Runs the application as a client (needs GUI user account).");            
            Console.WriteLine("  --install       Installs the application (needs root permissions).");            
            Console.WriteLine("  --uninstall     Uninstalls the application (needs root permissions).");            
        }
    }
}