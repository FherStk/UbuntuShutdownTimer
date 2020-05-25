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
using System.IO;
using System.Xml;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;
using DBus.DBus;

namespace UST
{   
    //Tested multi-server and multi-client; only the last registered server is processing the requests
    //All the registered clients are working ok, only 1 server can work ok.

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
                if(args.Where(x => x.Equals("--install")).Count() == 1) await Install();                          
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
       
        
        private static async Task Install(){
            Console.WriteLine("Installation requested: ", DateTime.Now.Year);
            InstallDbusPolicies();


            // Console.Write("  Removing the current config file... ");
            // File.Delete(dest);
            // Console.WriteLine("OK");
            
            // Console.Write("  Creating a new config file...       ");
            // File.Copy(source, dest);
            // Console.WriteLine("OK");

            Console.WriteLine();
            Console.Write("  Reloading dbus configuration...     ");
            var systemConnection = Connection.System;
            var dbusManager = systemConnection.CreateProxy<IDBus>("org.freedesktop.DBus", "/org/freedesktop/DBus");
            await dbusManager.ReloadConfigAsync();
            Console.WriteLine("OK"); 
        }

        

        private static void InstallDbusPolicies(){
            var filename = "system-local.conf";
            var source = Path.Combine("files", filename);
            var dest = Path.Combine("/etc/dbus-1", filename);

            Console.WriteLine("  Setting up the dbus policies:");            
            if(!File.Exists(dest)){
                Console.Write("    Creating the file {0}... ", dest);
                File.Copy(source, dest);
                Console.WriteLine("OK");
            }
            else{
                Console.Write("    Updating the file {0}... ", dest);
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(dest);

                var root = "busconfig";
                var nodeName = "policy";
                var attrName = "context";
                var attrValue = "default";
                
                var def = doc.DocumentElement.SelectSingleNode($"/{root}/{nodeName}[@{attrName}='{attrValue}']");
                if(def == null) CreateXmlNode(doc, doc.DocumentElement.SelectSingleNode($"/{root}"), nodeName, attrName, attrValue);
                
                nodeName = "allow";
                attrName = "own";
                attrValue = "net.xeill.elpuig.UST1";
                var allow = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                if(allow == null) CreateXmlNode(doc, def, nodeName, attrName, attrValue);

                attrName = "send_destination";
                var send = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                if(send == null) CreateXmlNode(doc, def, nodeName, attrName, attrValue);

                attrName = "receive_sender";
                var receive = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                if(receive == null) CreateXmlNode(doc, def, nodeName, attrName, attrValue);

                doc.Save(dest);
                Console.WriteLine("OK");
            } 
        }

        private static void CreateXmlNode(XmlDocument doc, XmlNode parent, string nodeName, string attrName, string attrValue){
            var node = doc.CreateElement(nodeName);                    
            var attr = doc.CreateAttribute(attrName);
            attr.Value = attrValue;
            node.Attributes.Append(attr);
            parent.AppendChild(node);
        }
    }
}